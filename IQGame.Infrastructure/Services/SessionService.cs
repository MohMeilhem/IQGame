using IQGame.Application.Interfaces;
using IQGame.Application.Models;
using IQGame.Application.Models.Session;
using IQGame.Infrastructure.Persistence;
using IQGame.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IQGame.Infrastructure.Services
{
    public class SessionService : ISessionService
    {
        private readonly IQGameDbContext _context;

        public SessionService(IQGameDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryWithQuestionsViewModel>> GetSessionQuestionsAsync(int sessionId)
        {
            var result = await _context.SessionQuestions
                .Where(sq => sq.SessionId == sessionId)
                .Include(sq => sq.Question)
                    .ThenInclude(q => q.Category)
                .Include(sq => sq.Question)
                    .ThenInclude(q => q.Answers)
                .ToListAsync();

            var grouped = result
                .GroupBy(sq => sq.Question.Category)
                .Select(g => new CategoryWithQuestionsViewModel
                {
                    CategoryId = g.Key.Id,
                    CategoryName = g.Key.Name,
                    CategoryImageUrl = g.Key.ImageUrl,
                    Questions = g.Select(q => new SessionQuestionViewModel
                    {
                        QuestionId = q.Question.Id,
                        Text = q.Question.Text,
                        Difficulty = q.Question.Difficulty,
                        Points = q.Question.Difficulty == 1 ? 250 : q.Question.Difficulty == 2 ? 500 : q.Question.Difficulty == 3 ? 750 : 0,
                        ImageUrl = q.Question.ImageUrl,
                        IsScored = q.IsScored,
                        CategoryId = q.Question.CategoryId,
                        CategoryDisableMCQ = q.Question.Category?.DisableMCQ ?? false,
                        Answers = q.Question.Answers?.Select(a => new AnswerViewModel
                        {
                            Id = a.Id,
                            Text = a.Text,
                            ImageUrl = a.ImageUrl,
                            IsCorrect = a.IsCorrect
                        }).ToList()
                    }).ToList()
                })
                .ToList();

            return grouped;
        }
        public async Task<bool> ScoreQuestionAsync(ScoreQuestionRequest request)
        {
            Console.WriteLine($"🔍 [SessionService.Backend] Starting ScoreQuestionAsync - SessionId: {request.SessionId}, TeamName: {request.TeamName}, QuestionId: {request.QuestionId}");

            // 1. Get session question
            var sessionQuestion = await _context.SessionQuestions
                .FirstOrDefaultAsync(sq => sq.SessionId == request.SessionId && sq.QuestionId == request.QuestionId);

            if (sessionQuestion == null)
            {
                Console.WriteLine($"❌ [SessionService.Backend] Session question not found for SessionId: {request.SessionId}, QuestionId: {request.QuestionId}");
                return false;
            }

            if (sessionQuestion.IsScored)
            {
                Console.WriteLine($"❌ [SessionService.Backend] Question already scored");
                return false;
            }

            // 2. Get question details
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId);

            if (question == null)
            {
                Console.WriteLine($"❌ [SessionService.Backend] Question not found for ID: {request.QuestionId}");
                return false;
            }

            Console.WriteLine($"🔍 [SessionService.Backend] Question found - Points: {question.Points}");

            // 3. Handle case where no team answered (TeamName is null or empty)
            if (string.IsNullOrWhiteSpace(request.TeamName))
            {
                Console.WriteLine($"🔍 [SessionService.Backend] No team answered, marking as scored with no team");
                
                // Consume double points even when no one answers
                var helpServiceForNoAnswer = new HelpService(_context);
                bool hasActiveDoublePointsForNoAnswer = await helpServiceForNoAnswer.HasActiveDoublePointsInSessionAsync(request.SessionId);
                
                if (hasActiveDoublePointsForNoAnswer)
                {
                    await helpServiceForNoAnswer.ConsumeDoublePointsForQuestionAsync(request.SessionId, request.QuestionId);
                    Console.WriteLine($"🔍 [SessionService.Backend] Double points consumed for question {request.QuestionId} even though no team answered");
                }
                
                sessionQuestion.IsScored = true;
                sessionQuestion.TeamAnswered = null;

                var saveResult = await _context.SaveChangesAsync() > 0;
                Console.WriteLine($"🔍 [SessionService.Backend] Save result for no team answer: {saveResult}");
                return saveResult;
            }

            // 4. Regular scoring logic for when a team answers
            var helpServiceForTeam = new HelpService(_context);
            bool hasActiveDoublePointsInSession = await helpServiceForTeam.HasActiveDoublePointsInSessionAsync(request.SessionId);

            Console.WriteLine($"🔍 [SessionService.Backend] Has active double points in session: {hasActiveDoublePointsInSession}");

            int points = question.Points;
            if (hasActiveDoublePointsInSession) 
            {
                points *= 2;
                // Consume the double points for this specific question (from any team that has them active)
                await helpServiceForTeam.ConsumeDoublePointsForQuestionAsync(request.SessionId, request.QuestionId);
                Console.WriteLine($"🔍 [SessionService.Backend] Double points consumed for question {request.QuestionId}");
            }

            Console.WriteLine($"🔍 [SessionService.Backend] Final points to award: {points}");

            // 5. Update team score
            var teamScore = await _context.TeamScores
                .FirstOrDefaultAsync(ts =>
                    ts.SessionId == request.SessionId &&
                    ts.TeamName == request.TeamName);

            if (teamScore == null)
            {
                Console.WriteLine($"❌ [SessionService.Backend] Team score not found for team: {request.TeamName}");
                return false;
            }

            Console.WriteLine($"🔍 [SessionService.Backend] Team score before update: {teamScore.Score}");
            teamScore.Score += points;
            Console.WriteLine($"🔍 [SessionService.Backend] Team score after update: {teamScore.Score}");

            sessionQuestion.IsScored = true;
            sessionQuestion.TeamAnswered = request.TeamName;

            var finalSaveResult = await _context.SaveChangesAsync() > 0;
            Console.WriteLine($"🔍 [SessionService.Backend] Final save result: {finalSaveResult}");
            
            // Reset help tools for next question (except double points and other session-wide tools)
            if (finalSaveResult)
            {
                // Only reset double points for the team that actually used them in this question
                if (hasActiveDoublePointsInSession)
                {
                    // The double points were already consumed above, so no need to reset them again
                    Console.WriteLine($"🔍 [SessionService.Backend] Double points already consumed for question {request.QuestionId}");
                }
            }
            
            return finalSaveResult;
        }

        public async Task<SessionResultViewModel> GetSessionResultsAsync(int sessionId)
        {
            Console.WriteLine($"🔍 [SessionService.Backend] Starting GetSessionResultsAsync for SessionId: {sessionId}");
            
            // 1. Get team scores
            var teams = await _context.TeamScores
                .Where(ts => ts.SessionId == sessionId)
                .ToListAsync();

            Console.WriteLine($"🔍 [SessionService.Backend] Found {teams.Count} teams");

            if (teams.Count != 2) 
            {
                Console.WriteLine($"❌ [SessionService.Backend] Expected 2 teams, found {teams.Count}");
                return null;
            }

            var team1 = teams[0];
            var team2 = teams[1];

            Console.WriteLine($"🔍 [SessionService.Backend] Team1: {team1.TeamName} (Score: {team1.Score}), Team2: {team2.TeamName} (Score: {team2.Score})");

            // 2. Get all scored questions in this session
            var sessionQuestions = await _context.SessionQuestions
                .Where(sq => sq.SessionId == sessionId && sq.IsScored)
                .Include(sq => sq.Question)
                    .ThenInclude(q => q.Category)
                .ToListAsync();

            Console.WriteLine($"🔍 [SessionService.Backend] Found {sessionQuestions.Count} scored questions");

            // 3. Group by category and calculate points per team
            var categoryBreakdown = sessionQuestions
                .GroupBy(sq => sq.Question.Category)
                .Select(g => new CategoryScoreBreakdown
                {
                    CategoryId = g.Key.Id,
                    CategoryName = g.Key.Name,
                    Team1Points = g.Where(q => q.TeamAnswered == team1.TeamName).Sum(q => q.Question.Points),
                    Team2Points = g.Where(q => q.TeamAnswered == team2.TeamName).Sum(q => q.Question.Points)
                })
                .ToList();

            Console.WriteLine($"🔍 [SessionService.Backend] Category breakdown calculated for {categoryBreakdown.Count} categories");

            // 4. Determine the winner
            string winner;
            if (team1.Score > team2.Score)
                winner = team1.TeamName;
            else if (team2.Score > team1.Score)
                winner = team2.TeamName;
            else
                winner = "تعادل";

            Console.WriteLine($"🔍 [SessionService.Backend] Winner determined: {winner}");

            // 5. Return result
            var result = new SessionResultViewModel
            {
                Team1 = new TeamScoreResult { Name = team1.TeamName, Score = team1.Score },
                Team2 = new TeamScoreResult { Name = team2.TeamName, Score = team2.Score },
                Winner = winner,
                Categories = categoryBreakdown
            };

            Console.WriteLine($"🔍 [SessionService.Backend] Final result - Team1: {result.Team1.Name} (Score: {result.Team1.Score}), Team2: {result.Team2.Name} (Score: {result.Team2.Score})");
            
            return result;
        }
        public async Task<GameStatusViewModel> GetGameStatusAsync(int sessionId)
        {
            Console.WriteLine($"🔍 [SessionService.Backend] Starting GetGameStatusAsync for SessionId: {sessionId}");
            
            var session = await _context.Sessions
                .Include(s => s.TeamScores)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null || session.TeamScores.Count != 2)
            {
                Console.WriteLine($"❌ [SessionService.Backend] Session not found or invalid team count - Session: {session != null}, TeamCount: {session?.TeamScores.Count}");
                return null;
            }

            var totalScored = await _context.SessionQuestions
                .CountAsync(sq => sq.SessionId == sessionId && sq.IsScored);

            Console.WriteLine($"🔍 [SessionService.Backend] Total scored questions: {totalScored}");

            // Get help tools that are still active (not consumed for current question)
            var usedHelps = await _context.UsedHelps
                .Where(h => h.SessionId == sessionId)
                .ToListAsync();
            
            var team1 = session.TeamScores.ElementAt(0);
            var team2 = session.TeamScores.ElementAt(1);

            Console.WriteLine($"🔍 [SessionService.Backend] Team1: {team1.TeamName} (Score: {team1.Score}), Team2: {team2.TeamName} (Score: {team2.Score})");

            var result = new GameStatusViewModel
            {
                TotalQuestionsAnswered = totalScored,
                GameFinished = totalScored >= 36,
                CurrentTurn = session.CurrentTurnTeam,
                SessionName = session.Name,
                Team1Name = team1.TeamName,
                Team2Name = team2.TeamName,
                Team1Score = team1.Score,
                Team2Score = team2.Score,
                Team1Help = new TeamHelpStatus
                {
                    TeamName = team1.TeamName,
                    KhiyaratUsed = usedHelps.Any(h => h.TeamName == team1.TeamName && h.HelpType == "خيارات"),
                    DoublePointsUsed = usedHelps.Any(h => h.TeamName == team1.TeamName && h.HelpType == "دبل"),
                    DoublePointsActive = usedHelps.Any(h => h.TeamName == team1.TeamName && h.HelpType == "دبل" && !h.IsConsumed),
                    TwoAnswersUsed = usedHelps.Any(h => h.TeamName == team1.TeamName && h.HelpType == "إجابتين")
                },
                Team2Help = new TeamHelpStatus
                {
                    TeamName = team2.TeamName,
                    KhiyaratUsed = usedHelps.Any(h => h.TeamName == team2.TeamName && h.HelpType == "خيارات"),
                    DoublePointsUsed = usedHelps.Any(h => h.TeamName == team2.TeamName && h.HelpType == "دبل"),
                    DoublePointsActive = usedHelps.Any(h => h.TeamName == team2.TeamName && h.HelpType == "دبل" && !h.IsConsumed),
                    TwoAnswersUsed = usedHelps.Any(h => h.TeamName == team2.TeamName && h.HelpType == "إجابتين")
                }
            };

            Console.WriteLine($"🔍 [SessionService.Backend] GameStatus result - GameFinished: {result.GameFinished}, TotalQuestionsAnswered: {result.TotalQuestionsAnswered}");
            Console.WriteLine($"🔍 [SessionService.Backend] Final scores - Team1: {result.Team1Score}, Team2: {result.Team2Score}");

            return result;
        }
        public async Task<bool> ChangeTurnAsync(int sessionId, string teamName)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return false;

            session.CurrentTurnTeam = teamName;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<string?> GetCurrentTurnAsync(int sessionId)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            return session?.CurrentTurnTeam;
        }

        public async Task<List<SessionSummaryDto>> GetAllSessionsWithCategoriesAsync()
        {
            var sessions = await _context.Sessions
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)
                        .ThenInclude(q => q.Category)
                .Include(s => s.TeamScores)
                .ToListAsync();

            var result = new List<SessionSummaryDto>();

            foreach (var session in sessions)
            {
                var categories = session.SessionQuestions
                    .Select(sq => sq.Question.Category)
                    .Distinct()
                    .Take(6)
                    .Select(cat => new CategorySummaryDto
                    {
                        CategoryId = cat.Id,
                        CategoryName = cat.Name,
                        CategoryImageUrl = cat.ImageUrl
                    })
                    .ToList();

                // Get question count for validation
                var questionCount = await GetSessionQuestionCountAsync(session.Id);

                // Get team names
                var team1Name = session.TeamScores.ElementAtOrDefault(0)?.TeamName ?? "Team 1";
                var team2Name = session.TeamScores.ElementAtOrDefault(1)?.TeamName ?? "Team 2";

                result.Add(new SessionSummaryDto
                {
                    SessionId = session.Id,
                    SessionName = session.Name,
                    Categories = categories,
                    TotalQuestions = questionCount,
                    IsValid = questionCount == 36, // 6 categories × 6 questions each
                    Team1Name = team1Name,
                    Team2Name = team2Name
                });
            }

            return result;
        }

        public async Task<int> GetSessionQuestionCountAsync(int sessionId)
        {
            var questionCount = await _context.SessionQuestions
                .Where(sq => sq.SessionId == sessionId)
                .CountAsync();
            
            return questionCount;
        }

        public async Task<bool> ValidateSessionQuestionsAsync(int sessionId)
        {
            var questionCount = await GetSessionQuestionCountAsync(sessionId);
            return questionCount == 36; // 6 categories × 6 questions each
        }

        public async Task<int> DeleteSessionsContainingCategoryAsync(int categoryId)
        {
            // Find all sessions that contain questions from this category
            var sessionsToDelete = await _context.SessionQuestions
                .Where(sq => sq.Question.CategoryId == categoryId)
                .Select(sq => sq.SessionId)
                .Distinct()
                .ToListAsync();

            if (!sessionsToDelete.Any())
            {
                return 0; // No sessions to delete
            }

            // Delete related data for these sessions
            foreach (var sessionId in sessionsToDelete)
            {
                // Delete SessionQuestions
                var sessionQuestions = await _context.SessionQuestions
                    .Where(sq => sq.SessionId == sessionId)
                    .ToListAsync();
                _context.SessionQuestions.RemoveRange(sessionQuestions);

                // Delete TeamScores
                var teamScores = await _context.TeamScores
                    .Where(ts => ts.SessionId == sessionId)
                    .ToListAsync();
                _context.TeamScores.RemoveRange(teamScores);

                // Delete UsedHelps
                var usedHelps = await _context.UsedHelps
                    .Where(uh => uh.SessionId == sessionId)
                    .ToListAsync();
                _context.UsedHelps.RemoveRange(usedHelps);

                // Delete the Session itself
                var session = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.Id == sessionId);
                if (session != null)
                {
                    _context.Sessions.Remove(session);
                }
            }

            // Save all changes
            var result = await _context.SaveChangesAsync();
            return sessionsToDelete.Count;
        }

        public async Task<bool> ResetSessionAsync(int sessionId, string newSessionName, string team1Name, string team2Name)
        {
            try
            {
                var session = await _context.Sessions
                    .Include(s => s.TeamScores)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (session == null)
                    return false;

                // Update session name
                session.Name = newSessionName;
                session.CurrentTurnTeam = team1Name; // Start with team 1

                // Reset team scores
                foreach (var teamScore in session.TeamScores)
                {
                    teamScore.Score = 0;
                }

                // Update team names if they changed
                if (session.TeamScores.Count >= 2)
                {
                    session.TeamScores.ElementAt(0).TeamName = team1Name;
                    session.TeamScores.ElementAt(1).TeamName = team2Name;
                }

                // Reset all session questions (mark as not scored)
                var sessionQuestions = await _context.SessionQuestions
                    .Where(sq => sq.SessionId == sessionId)
                    .ToListAsync();

                foreach (var sq in sessionQuestions)
                {
                    sq.IsScored = false;
                    sq.TeamAnswered = null;
                }

                // Delete all used helps for this session
                var usedHelps = await _context.UsedHelps
                    .Where(uh => uh.SessionId == sessionId)
                    .ToListAsync();
                _context.UsedHelps.RemoveRange(usedHelps);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SessionService.Backend] Error resetting session {sessionId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CategoryAvailability>> GetCategoriesAvailabilityAsync()
        {
            Console.WriteLine($"🔍 [SessionService.Backend] Starting GetCategoriesAvailabilityAsync");
            
            // Get all categories with their questions and groups
            var categories = await _context.Categories
                .Include(c => c.Questions)
                .Include(c => c.Group)
                .ToListAsync();

            // Get all used question IDs from any session
            var usedQuestionIds = await _context.SessionQuestions
                .Select(sq => sq.QuestionId)
                .Distinct()
                .ToListAsync();

            Console.WriteLine($"🔍 [SessionService.Backend] Found {usedQuestionIds.Count} used questions across all sessions");

            var result = new List<CategoryAvailability>();

            foreach (var category in categories)
            {
                var totalQuestions = category.Questions?.Count ?? 0;
                var easyQuestions = category.Questions?.Where(q => q.Difficulty == 1).ToList() ?? new List<Question>();
                var mediumQuestions = category.Questions?.Where(q => q.Difficulty == 2).ToList() ?? new List<Question>();
                var hardQuestions = category.Questions?.Where(q => q.Difficulty == 3).ToList() ?? new List<Question>();

                // Calculate available questions (not used in any session)
                var easyQuestionsAvailable = easyQuestions.Where(q => !usedQuestionIds.Contains(q.Id)).Count();
                var mediumQuestionsAvailable = mediumQuestions.Where(q => !usedQuestionIds.Contains(q.Id)).Count();
                var hardQuestionsAvailable = hardQuestions.Where(q => !usedQuestionIds.Contains(q.Id)).Count();

                // Calculate used questions
                var usedQuestions = category.Questions?.Where(q => usedQuestionIds.Contains(q.Id)).Count() ?? 0;

                // Calculate available games
                // Each game needs: 2 easy (250 points) + 2 medium (500 points) + 2 hard (750 points) = 6 questions
                var availableGames = Math.Min(
                    Math.Min(easyQuestionsAvailable / 2, mediumQuestionsAvailable / 2),
                    hardQuestionsAvailable / 2
                );

                var availability = new CategoryAvailability
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    CategoryImageUrl = category.ImageUrl,
                    GroupId = category.GroupId,
                    GroupName = category.Group?.Name,
                    AvailableGames = availableGames,
                    TotalQuestions = totalQuestions,
                    UsedQuestions = usedQuestions,
                    EasyQuestionsAvailable = easyQuestionsAvailable,
                    MediumQuestionsAvailable = mediumQuestionsAvailable,
                    HardQuestionsAvailable = hardQuestionsAvailable,
                    EasyQuestionsTotal = easyQuestions.Count,
                    MediumQuestionsTotal = mediumQuestions.Count,
                    HardQuestionsTotal = hardQuestions.Count
                };

                result.Add(availability);

                Console.WriteLine($"🔍 [SessionService.Backend] Category {category.Name}: {availableGames} games available ({easyQuestionsAvailable} easy, {mediumQuestionsAvailable} medium, {hardQuestionsAvailable} hard available)");
            }

            Console.WriteLine($"🔍 [SessionService.Backend] Completed GetCategoriesAvailabilityAsync with {result.Count} categories");
            return result;
        }
    }
}

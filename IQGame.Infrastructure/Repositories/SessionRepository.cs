using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using IQGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using IQGame.Application.Models.Session;


namespace IQGame.Infrastructure.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly IQGameDbContext _context;

        public SessionRepository(IQGameDbContext context)
        {
            _context = context;
        }

        public async Task<Session> CreateSessionAsync(string sessionName, string team1, string team2, List<int> categoryIds)
        {
            // 1. Create session
            var session = new Session 
            { 
                Name = sessionName,
                CurrentTurnTeam = team1 // Set initial turn to team1
            };
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync(); // Save to get session.Id

            // 2. Use provided categories
            var categories = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();

            var sessionQuestions = new List<SessionQuestion>();
            var invalidCategories = new List<string>();

            // Get all question IDs that have already been used in ANY session
            var usedQuestionIds = await _context.SessionQuestions
                .Select(sq => sq.QuestionId)
                .Distinct()
                .ToListAsync();

            foreach (var category in categories)
            {
                // 3. Get 2 easy, 2 medium, 2 hard questions from each category, excluding used ones
                var easy = await _context.Questions
                    .Where(q => q.CategoryId == category.Id && q.Difficulty == 1 && !usedQuestionIds.Contains(q.Id))
                    .OrderBy(q => Guid.NewGuid())
                    .Take(2)
                    .ToListAsync();

                var medium = await _context.Questions
                    .Where(q => q.CategoryId == category.Id && q.Difficulty == 2 && !usedQuestionIds.Contains(q.Id))
                    .OrderBy(q => Guid.NewGuid())
                    .Take(2)
                    .ToListAsync();

                var hard = await _context.Questions
                    .Where(q => q.CategoryId == category.Id && q.Difficulty == 3 && !usedQuestionIds.Contains(q.Id))
                    .OrderBy(q => Guid.NewGuid())
                    .Take(2)
                    .ToListAsync();

                // Check if we got enough questions for each difficulty level
                if (easy.Count < 2 || medium.Count < 2 || hard.Count < 2)
                {
                    invalidCategories.Add(category.Name);
                }
                else
                {
                    var selectedQuestions = easy.Concat(medium).Concat(hard);

                    foreach (var question in selectedQuestions)
                    {
                        sessionQuestions.Add(new SessionQuestion
                        {
                            SessionId = session.Id,
                            QuestionId = question.Id
                        });
                    }
                }
            }

            // If any categories are invalid, rollback and throw exception
            if (invalidCategories.Any())
            {
                // Rollback session creation
                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
                throw new InvalidOperationException($"Categories {string.Join(", ", invalidCategories)} don't have enough available questions");
            }

            // 4. Save SessionQuestions
            await _context.SessionQuestions.AddRangeAsync(sessionQuestions);

            // 5. Save team scores
            var teamScores = new List<TeamScore>
            {
                new TeamScore { SessionId = session.Id, TeamName = team1, Score = 0 },
                new TeamScore { SessionId = session.Id, TeamName = team2, Score = 0 }
            };

            await _context.TeamScores.AddRangeAsync(teamScores);
            await _context.SaveChangesAsync();

            return session;
        }
    }
}

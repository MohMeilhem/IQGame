using IQGame.Application.Models.Help;
using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using IQGame.Application.Interfaces;
using IQGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IQGame.Infrastructure.Services
{
    public class HelpService : IHelpService
    {
        private readonly IQGameDbContext _context;

        public HelpService(IQGameDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasUsedHelpAsync(int sessionId, string teamName, string helpType)
        {
            // For double points, check if it's consumed. For other help tools, just check if they exist (session-wide)
            if (helpType == "دبل")
            {
                return await _context.UsedHelps.AnyAsync(h =>
                    h.SessionId == sessionId &&
                    h.TeamName == teamName &&
                    h.HelpType == helpType &&
                    !h.IsConsumed
                );
            }
            else
            {
                // For other help tools (خيارات, إجابتين), check if they exist at all (session-wide)
                return await _context.UsedHelps.AnyAsync(h =>
                    h.SessionId == sessionId &&
                    h.TeamName == teamName &&
                    h.HelpType == helpType
                );
            }
        }

        public async Task<bool> UseHelpAsync(int sessionId, string teamName, string helpType, int? questionId = null)
        {
            // 1. Check if already used
            if (await HasUsedHelpAsync(sessionId, teamName, helpType))
                return false;

            // 2. For "خيارات" help, check if the question's category has MCQ disabled
            if (helpType == "خيارات" && questionId.HasValue)
            {
                var question = await _context.Questions
                    .Include(q => q.Category)
                    .FirstOrDefaultAsync(q => q.Id == questionId.Value);

                if (question?.Category?.DisableMCQ == true)
                {
                    return false; // MCQ is disabled for this category
                }
            }

            // 3. Log usage
            var help = new UsedHelp
            {
                SessionId = sessionId,
                TeamName = teamName,
                HelpType = helpType,
                QuestionId = questionId,
                IsConsumed = false // Will be marked as consumed after question is scored
            };

            _context.UsedHelps.Add(help);

            // 4. If "خيارات" → deduct 150 points from team
            if (helpType == "خيارات")
            {
                var teamScore = await _context.TeamScores
                    .FirstOrDefaultAsync(ts => ts.SessionId == sessionId && ts.TeamName == teamName);

                if (teamScore != null)
                {
                    teamScore.Score -= 150;

                    // prevent negative score (optional)
                    if (teamScore.Score < 0)
                        teamScore.Score = 0;
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ConsumeDoublePointsAsync(int sessionId, string teamName, int questionId)
        {
            // Find the double points help for this team and mark it as consumed for this specific question
            var doublePointsHelp = await _context.UsedHelps
                .FirstOrDefaultAsync(h => 
                    h.SessionId == sessionId && 
                    h.TeamName == teamName && 
                    h.HelpType == "دبل" && 
                    !h.IsConsumed);

            if (doublePointsHelp != null)
            {
                doublePointsHelp.IsConsumed = true;
                doublePointsHelp.QuestionId = questionId;
                return await _context.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> ConsumeDoublePointsForQuestionAsync(int sessionId, int questionId)
        {
            // Find any active double points in the session and mark them as consumed for this specific question
            var activeDoublePoints = await _context.UsedHelps
                .Where(h => 
                    h.SessionId == sessionId && 
                    h.HelpType == "دبل" && 
                    !h.IsConsumed)
                .ToListAsync();

            if (activeDoublePoints.Any())
            {
                foreach (var doublePoints in activeDoublePoints)
                {
                    doublePoints.IsConsumed = true;
                    doublePoints.QuestionId = questionId;
                }
                return await _context.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> HasActiveDoublePointsAsync(int sessionId, string teamName)
        {
            // Check if team has double points activated but not yet consumed
            return await _context.UsedHelps.AnyAsync(h =>
                h.SessionId == sessionId &&
                h.TeamName == teamName &&
                h.HelpType == "دبل" &&
                !h.IsConsumed
            );
        }

        public async Task<bool> HasActiveDoublePointsInSessionAsync(int sessionId)
        {
            // Check if any team in the session has double points activated but not yet consumed
            return await _context.UsedHelps.AnyAsync(h =>
                h.SessionId == sessionId &&
                h.HelpType == "دبل" &&
                !h.IsConsumed
            );
        }

        public async Task<HelpStatusViewModel> GetHelpStatusAsync(int sessionId, string teamName)
        {
            // Get all help tools for the team
            var allUsedHelps = await _context.UsedHelps
                .Where(h => h.SessionId == sessionId && h.TeamName == teamName)
                .ToListAsync();

            // For double points, check if it's not consumed. For others, check if they exist at all
            var activeDoublePoints = allUsedHelps.Any(h => h.HelpType == "دبل" && !h.IsConsumed);
            var hasKhiyarat = allUsedHelps.Any(h => h.HelpType == "خيارات");
            var hasTwoAnswers = allUsedHelps.Any(h => h.HelpType == "إجابتين");

            return new HelpStatusViewModel
            {
                KhiyaratUsed = hasKhiyarat,
                DoublePointsUsed = allUsedHelps.Any(h => h.HelpType == "دبل"),
                TwoAnswersUsed = hasTwoAnswers
            };
        }

    }
}

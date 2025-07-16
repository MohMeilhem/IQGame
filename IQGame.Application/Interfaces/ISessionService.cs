using IQGame.Application.Models;
using IQGame.Application.Models.Session;
using IQGame.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IQGame.Application.Interfaces
{
    public interface ISessionService
    {
        Task<List<CategoryWithQuestionsViewModel>> GetSessionQuestionsAsync(int sessionId);
        Task<bool> ScoreQuestionAsync(ScoreQuestionRequest request);
        Task<SessionResultViewModel> GetSessionResultsAsync(int sessionId);
        Task<GameStatusViewModel> GetGameStatusAsync(int sessionId);
        Task<bool> ChangeTurnAsync(int sessionId, string teamName);
        Task<string?> GetCurrentTurnAsync(int sessionId);

        // New for resume session feature
        Task<List<SessionSummaryDto>> GetAllSessionsWithCategoriesAsync();
        Task<int> GetSessionQuestionCountAsync(int sessionId);
        Task<bool> ValidateSessionQuestionsAsync(int sessionId);
        
        // New for category deletion
        Task<int> DeleteSessionsContainingCategoryAsync(int categoryId);
        Task<bool> ResetSessionAsync(int sessionId, string newSessionName, string team1Name, string team2Name);
        
        // New for category availability
        Task<List<CategoryAvailability>> GetCategoriesAvailabilityAsync();
    }
}

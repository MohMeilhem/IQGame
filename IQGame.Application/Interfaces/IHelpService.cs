using IQGame.Application.Models.Help;


namespace IQGame.Application.Interfaces
{

    public interface IHelpService
    {
        Task<bool> UseHelpAsync(int sessionId, string teamName, string helpType, int? questionId = null);
        Task<bool> HasUsedHelpAsync(int sessionId, string teamName, string helpType);
        Task<HelpStatusViewModel> GetHelpStatusAsync(int sessionId, string teamName);
        Task<bool> ConsumeDoublePointsAsync(int sessionId, string teamName, int questionId);
        Task<bool> ConsumeDoublePointsForQuestionAsync(int sessionId, int questionId);
        Task<bool> HasActiveDoublePointsAsync(int sessionId, string teamName);
        Task<bool> HasActiveDoublePointsInSessionAsync(int sessionId);
    }
}

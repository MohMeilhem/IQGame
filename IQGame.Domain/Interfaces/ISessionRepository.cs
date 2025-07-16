using IQGame.Shared.Models;

namespace IQGame.Domain.Interfaces
{
    public interface ISessionRepository
    {
        Task<Session> CreateSessionAsync(string sessionName, string team1, string team2, List<int> categoryIds);
    }
}

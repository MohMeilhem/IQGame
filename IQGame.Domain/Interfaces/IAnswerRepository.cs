using IQGame.Shared.Models;

namespace IQGame.Domain.Interfaces
{
    public interface IAnswerRepository
    {
        Task<IEnumerable<Answer>> GetAllAsync();
        Task<Answer?> GetByIdAsync(int id);
        Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId);
        Task AddAsync(Answer answer);
        Task<bool> SaveChangesAsync();
    }
}

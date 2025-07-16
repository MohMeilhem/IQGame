using IQGame.Shared.Models;

namespace IQGame.Domain.Interfaces
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetAllAsync();
        Task<Question?> GetByIdAsync(int id);
        Task<IEnumerable<Question>> GetByCategoryAsync(int categoryId);
        Task AddAsync(Question question);
        Task<bool> SaveChangesAsync();
    }
}

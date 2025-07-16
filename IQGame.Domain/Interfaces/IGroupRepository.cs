using IQGame.Shared.Models;

namespace IQGame.Domain.Interfaces
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAllAsync();
        Task<IEnumerable<Group>> GetActiveAsync();
        Task<Group?> GetByIdAsync(int id);
        Task<Group?> GetByNameAsync(string name);
        Task AddAsync(Group group);
        Task UpdateAsync(Group group);
        Task DeleteAsync(Group group);
        Task<bool> SaveChangesAsync();
    }
} 
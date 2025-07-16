using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using IQGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IQGame.Infrastructure.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly IQGameDbContext _context;

        public GroupRepository(IQGameDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await _context.Groups
                .OrderBy(g => g.DisplayOrder)
                .ThenBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Group>> GetActiveAsync()
        {
            return await _context.Groups
                .Where(g => g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .ThenBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<Group?> GetByIdAsync(int id)
        {
            return await _context.Groups
                .Include(g => g.Categories)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Group?> GetByNameAsync(string name)
        {
            return await _context.Groups
                .FirstOrDefaultAsync(g => g.Name == name);
        }

        public async Task AddAsync(Group group)
        {
            await _context.Groups.AddAsync(group);
        }

        public async Task UpdateAsync(Group group)
        {
            _context.Entry(group).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Group group)
        {
            _context.Groups.Remove(group);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
} 
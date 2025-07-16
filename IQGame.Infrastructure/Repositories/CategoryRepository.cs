using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using IQGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IQGame.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IQGameDbContext _context;

        public CategoryRepository(IQGameDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Questions)
                .Include(c => c.Group)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Questions)
                .Include(c => c.Group)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Question>> GetQuestionsByCategoryIdAsync(int categoryId)
        {
            return await _context.Questions
                .Where(q => q.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Entry(category).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

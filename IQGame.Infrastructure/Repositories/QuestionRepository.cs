using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using IQGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IQGame.Infrastructure.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IQGameDbContext _context;

        public QuestionRepository(IQGameDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Question>> GetAllAsync()
        {
            return await _context.Questions
                .Include(q => q.Category)
                .Include(q => q.Answers)
                .ToListAsync();
        }

        public async Task<Question?> GetByIdAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.Category)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IEnumerable<Question>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Questions
                .Include(q => q.Category)
                .Include(q => q.Answers)
                .Where(q => q.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task AddAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

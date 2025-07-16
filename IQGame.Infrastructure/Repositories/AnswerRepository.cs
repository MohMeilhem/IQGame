using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using IQGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IQGame.Infrastructure.Repositories
{
    public class AnswerRepository : IAnswerRepository
    {
        private readonly IQGameDbContext _context;

        public AnswerRepository(IQGameDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Answer>> GetAllAsync()
        {
            return await _context.Answers
                .Include(a => a.Question)
                .ToListAsync();
        }

        public async Task<Answer?> GetByIdAsync(int id)
        {
            return await _context.Answers
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId)
        {
            return await _context.Answers
                .Include(a => a.Question)
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();
        }

        public async Task AddAsync(Answer answer)
        {
            await _context.Answers.AddAsync(answer);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IQGame.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionRepository _repo;

        public QuestionsController(IQuestionRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var questions = await _repo.GetAllAsync();
            return Ok(questions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var question = await _repo.GetByIdAsync(id);
            if (question == null) return NotFound();
            return Ok(question);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var questions = await _repo.GetByCategoryAsync(categoryId);
            return Ok(questions);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Question question)
        {
            await _repo.AddAsync(question);
            var success = await _repo.SaveChangesAsync();
            if (!success) return BadRequest("Could not save question.");
            return Ok(question);
        }
    }
}

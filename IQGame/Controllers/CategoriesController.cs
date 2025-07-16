using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IQGame.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _repo;

        public CategoriesController(ICategoryRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _repo.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpGet("{id}/questions")]
        public async Task<IActionResult> GetQuestions(int id)
        {
            var questions = await _repo.GetQuestionsByCategoryIdAsync(id);
            if (questions == null) return NotFound();
            return Ok(questions);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            await _repo.AddAsync(category);
            var success = await _repo.SaveChangesAsync();
            if (!success) return BadRequest("Could not save category.");
            return Ok(category);
        }

    }
}

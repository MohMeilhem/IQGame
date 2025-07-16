using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IQGame.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerRepository _repo;

        public AnswersController(IAnswerRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var answers = await _repo.GetAllAsync();
            return Ok(answers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var answer = await _repo.GetByIdAsync(id);
            if (answer == null) return NotFound();
            return Ok(answer);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Answer answer)
        {
            await _repo.AddAsync(answer);
            var success = await _repo.SaveChangesAsync();
            if (!success) return BadRequest("Could not save answer.");
            return Ok(answer);
        }
    }
}

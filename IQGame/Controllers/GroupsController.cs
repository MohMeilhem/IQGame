using IQGame.Shared.Models;
using IQGame.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IQGame.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupRepository _repo;

        public GroupsController(IGroupRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var groups = await _repo.GetActiveAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var group = await _repo.GetByIdAsync(id);
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpGet("{id}/categories")]
        public async Task<IActionResult> GetCategories(int id)
        {
            var group = await _repo.GetByIdAsync(id);
            if (group == null) return NotFound();
            return Ok(group.Categories);
        }
    }
} 
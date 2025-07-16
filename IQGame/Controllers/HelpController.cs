using IQGame.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IQGame.Application.Models;
using IQGame.Application.Interfaces;

namespace IQGame.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpController : ControllerBase
    {
        private readonly IHelpService _helpService;

        public HelpController(IHelpService helpService)
        {
            _helpService = helpService;
        }

        [HttpPost("use")]
        public async Task<IActionResult> UseHelp([FromBody] UseHelpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TeamName) || string.IsNullOrWhiteSpace(request.HelpType))
                return BadRequest("Missing required fields.");

            var success = await _helpService.UseHelpAsync(request.SessionId, request.TeamName, request.HelpType, request.QuestionId);
            if (!success)
                return BadRequest("Help has already been used by this team.");

            return Ok("Help tool used successfully.");
        }
        [HttpGet("status")]
        public async Task<IActionResult> GetHelpStatus([FromQuery] int sessionId, [FromQuery] string teamName)
        {
            if (sessionId <= 0 || string.IsNullOrWhiteSpace(teamName))
                return BadRequest("Session ID and team name are required.");

            var status = await _helpService.GetHelpStatusAsync(sessionId, teamName);
            return Ok(status);
        }

    }
}

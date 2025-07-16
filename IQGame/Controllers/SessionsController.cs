using IQGame.Application.Interfaces;
using IQGame.Application.Models;
using IQGame.Application.Models.Session;
using IQGame.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IQGame.Infrastructure.Persistence;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IQGame.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionRepository _repo;
        private readonly ISessionService _sessionService;
        private readonly IQGameDbContext _context;

        public SessionsController(ISessionRepository repo, ISessionService sessionService, IQGameDbContext context)
        {
            _repo = repo;
            _sessionService = sessionService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSessionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SessionName) ||
                string.IsNullOrWhiteSpace(request.Team1) ||
                string.IsNullOrWhiteSpace(request.Team2))
            {
                return BadRequest("Session name and team names are required.");
            }

            if (request.CategoryIds == null || request.CategoryIds.Count != 6)
            {
                return BadRequest("Exactly 6 category IDs must be provided.");
            }

            // Validate that selected categories have enough questions
            var validationResult = await ValidateCategoriesForSession(request.CategoryIds);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { error = "Invalid categories", message = validationResult.Message });
            }

            var session = await _repo.CreateSessionAsync(request.SessionName, request.Team1, request.Team2, request.CategoryIds);

            return Ok(new
            {
                sessionId = session.Id,
                request.SessionName,
                request.Team1,
                request.Team2,
                request.CategoryIds
            });
        }

        private async Task<CategoryValidationResult> ValidateCategoriesForSession(List<int> categoryIds)
        {
            try
            {
                var totalQuestions = 0;
                var invalidCategories = new List<string>();

                foreach (var categoryId in categoryIds)
                {
                    var category = await _context.Categories
                        .Include(c => c.Questions)
                        .FirstOrDefaultAsync(c => c.Id == categoryId);

                    if (category == null)
                    {
                        invalidCategories.Add($"Category ID {categoryId} not found");
                        continue;
                    }

                    var questionCount = category.Questions?.Count ?? 0;
                    if (questionCount < 6)
                    {
                        invalidCategories.Add($"{category.Name} ({questionCount}/6 questions)");
                    }

                    totalQuestions += questionCount;
                }

                if (invalidCategories.Any())
                {
                    return new CategoryValidationResult
                    {
                        IsValid = false,
                        Message = $"The following categories don't have 6 questions each:\n{string.Join("\n", invalidCategories)}"
                    };
                }

                if (totalQuestions < 36)
                {
                    return new CategoryValidationResult
                    {
                        IsValid = false,
                        Message = $"Total questions ({totalQuestions}) is less than required (36 questions)"
                    };
                }

                return new CategoryValidationResult
                {
                    IsValid = true,
                    Message = "All categories are valid"
                };
            }
            catch (Exception ex)
            {
                return new CategoryValidationResult
                {
                    IsValid = false,
                    Message = $"Error validating categories: {ex.Message}"
                };
            }
        }

        private class CategoryValidationResult
        {
            public bool IsValid { get; set; }
            public string Message { get; set; }
        }

        // 🆕 NEW: Get assigned questions for a session
        [HttpGet("{sessionId}/questions")]
        public async Task<IActionResult> GetQuestions(int sessionId)
        {
            var result = await _sessionService.GetSessionQuestionsAsync(sessionId);
            if (result == null || result.Count == 0)
                return NotFound("No questions found for this session.");

            return Ok(result);
        }

        [HttpPost("{sessionId}/score-question")]
        public async Task<IActionResult> ScoreQuestion(int sessionId, [FromBody] ScoreQuestionRequest request)
        {
            if (request.SessionId != sessionId || request.QuestionId <= 0)
                return BadRequest("Invalid session or question ID.");

            var result = await _sessionService.ScoreQuestionAsync(request);
            if (!result) return BadRequest("Could not score question.");

            return Ok("Score updated successfully.");
        }
        [HttpGet("{sessionId}/results")]
        public async Task<IActionResult> GetResults(int sessionId)
        {
            var result = await _sessionService.GetSessionResultsAsync(sessionId);
            if (result == null)
                return NotFound("Session not found or incomplete.");

            return Ok(result);
        }
        [HttpGet("{sessionId}/status")]
        public async Task<IActionResult> GetGameStatus(int sessionId)
        {
            var status = await _sessionService.GetGameStatusAsync(sessionId);
            if (status == null)
                return NotFound("Session not found.");

            return Ok(status);
        }
        [HttpGet("{sessionId}/current-turn")]
        public async Task<IActionResult> GetCurrentTurn(int sessionId)
        {
            try
            {
                var turn = await _sessionService.GetCurrentTurnAsync(sessionId);
                if (turn == null)
                    return NotFound("Session not found.");

                return Ok(new { currentTurn = turn });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }


        [HttpPost("{sessionId}/change-turn")]
        public async Task<IActionResult> ChangeTurn(int sessionId, [FromQuery] string teamName)
        {
            if (string.IsNullOrWhiteSpace(teamName))
                return BadRequest("Team name is required.");

            var result = await _sessionService.ChangeTurnAsync(sessionId, teamName);
            if (!result)
                return NotFound("Session not found or update failed.");

            return Ok("Turn updated.");
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllSessions()
        {
            // Get all sessions with their categories
            var sessions = await _sessionService.GetAllSessionsWithCategoriesAsync();
            return Ok(sessions);
        }

        [HttpGet("{sessionId}/validate")]
        public async Task<IActionResult> ValidateSession(int sessionId)
        {
            try
            {
                var isValid = await _sessionService.ValidateSessionQuestionsAsync(sessionId);
                var questionCount = await _sessionService.GetSessionQuestionCountAsync(sessionId);
                
                return Ok(new
                {
                    SessionId = sessionId,
                    TotalQuestions = questionCount,
                    IsValid = isValid,
                    ExpectedQuestions = 36,
                    Message = isValid ? "Session is valid with 36 questions" : $"Session has {questionCount} questions, expected 36"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("{sessionId}/update-score")]
        public async Task<IActionResult> UpdateScore(int sessionId, [FromBody] UpdateScoreRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Team))
                return BadRequest("Invalid request.");

            var teamScore = await _context.TeamScores.FirstOrDefaultAsync(ts => ts.SessionId == sessionId && ts.TeamName == request.Team);
            if (teamScore == null)
                return NotFound("Team not found in this session.");

            teamScore.Score += request.ScoreChange;
            if (teamScore.Score < 0) teamScore.Score = 0;
            if (teamScore.Score > 10500) teamScore.Score = 10500;

            await _context.SaveChangesAsync();
            return Ok(new { team = request.Team, score = teamScore.Score });
        }

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetSession(int sessionId)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null)
                return NotFound("Session not found.");
            var teamScores = await _context.TeamScores.Where(ts => ts.SessionId == sessionId).ToListAsync();
            return Ok(new {
                SessionId = session.Id,
                SessionName = session.Name,
                Team1 = teamScores.Count > 0 ? teamScores[0].TeamName : null,
                Team2 = teamScores.Count > 1 ? teamScores[1].TeamName : null
            });
        }
    }

    public class UpdateScoreRequest
    {
        public string Team { get; set; }
        public int ScoreChange { get; set; }
    }
}

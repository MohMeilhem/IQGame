using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using IQGame.Shared.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using IQGame.Infrastructure.Persistence;

namespace IQGame.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IQGameDbContext _context;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IQGameDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var identityUser = new IdentityUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(identityUser, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Create app user
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                IdentityUserId = identityUser.Id, // FIXED: use Id
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.GameUsers.Add(user);
            await _context.SaveChangesAsync();

            // Grant 1 free game on registration (create a UserPlan)
            var freePlan = _context.Plans.FirstOrDefault(p => p.Name == "Free Trial");
            if (freePlan == null)
            {
                freePlan = new Plan
                {
                    Name = "Free Trial",
                    GamesCount = 1,
                    Price = 0,
                    IsActive = true
                };
                _context.Plans.Add(freePlan);
                await _context.SaveChangesAsync();
            }
            var userPlan = new UserPlan
            {
                UserId = user.Id,
                PlanId = freePlan.Id,
                GamesRemaining = 1,
                PurchaseDate = DateTime.UtcNow,
                PaymentStatus = "Free",
                StripeSessionId = null,
                ExpiryDate = null,
                IsActive = true
            };
            _context.UserPlans.Add(userPlan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful. 1 free game granted." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid login attempt.");

            var identityUser = await _userManager.FindByNameAsync(model.Username);
            var user = _context.GameUsers.FirstOrDefault(u => u.IdentityUserId == identityUser.Id);
            if (user == null)
                return Unauthorized("User not found.");

            // TODO: Return JWT or session info as needed
            return Ok(new { message = "Login successful", userId = user.Id });
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
} 
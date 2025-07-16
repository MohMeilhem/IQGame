using Microsoft.AspNetCore.Mvc;
using IQGame.Shared.Models;
using System.Linq;
using IQGame.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;
using Stripe;
using System.Collections.Generic;
using System;

namespace IQGame.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly IQGameDbContext _context;
        private readonly IConfiguration _configuration;
        public PlansController(IQGameDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var plans = _context.Plans.Where(p => p.IsActive).ToList();
            return Ok(plans);
        }

        [HttpPost("create-checkout-session")]
        public IActionResult CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            var plan = _context.Plans.FirstOrDefault(p => p.Id == request.PlanId && p.IsActive);
            if (plan == null)
                return NotFound("Plan not found.");

            var user = _context.GameUsers.FirstOrDefault(u => u.Id == request.UserId);
            if (user == null)
                return NotFound("User not found.");

            var stripeApiKey = _configuration["Stripe:SecretKey"];
            StripeConfiguration.ApiKey = stripeApiKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(plan.Price * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = plan.Name
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = "https://yourdomain.com/payment-success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://yourdomain.com/payment-cancelled",
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", request.UserId.ToString() },
                    { "PlanId", request.PlanId.ToString() }
                }
            };

            var service = new SessionService();
            var session = service.Create(options);

            // Create a UserPlan with status 'Pending' and store the Stripe session ID
            var userPlan = new UserPlan
            {
                UserId = user.Id,
                PlanId = plan.Id,
                GamesRemaining = plan.GamesCount,
                PurchaseDate = DateTime.UtcNow,
                PaymentStatus = "Pending",
                StripeSessionId = session.Id,
                ExpiryDate = null,
                IsActive = false
            };
            _context.UserPlans.Add(userPlan);
            _context.SaveChanges();

            return Ok(new { url = session.Url });
        }

        public class CreateCheckoutSessionRequest
        {
            public int UserId { get; set; }
            public int PlanId { get; set; }
        }
    }
} 
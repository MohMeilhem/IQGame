using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using IQGame.Infrastructure.Persistence;
using IQGame.Shared.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;

namespace IQGame.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IQGameDbContext _context;
        private readonly IConfiguration _configuration;
        public StripeWebhookController(IQGameDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = "whsec_your_placeholder_secret_here"; // TODO: configure this
            Event stripeEvent;
            try
            {
                var signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);
            }
            catch (StripeException e)
            {
                return BadRequest($"Stripe webhook error: {e.Message}");
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                if (session != null)
                {
                    var stripeSessionId = session.Id;
                    var userPlan = _context.UserPlans.FirstOrDefault(up => up.StripeSessionId == stripeSessionId);
                    if (userPlan != null)
                    {
                        userPlan.PaymentStatus = "Paid";
                        userPlan.IsActive = true;
                        _context.SaveChanges();
                    }
                }
            }

            // Handle other event types as needed

            return Ok();
        }
    }
} 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IQGame.Infrastructure.Persistence;
using System.Linq;

namespace IQGame.Admin.Controllers
{
    public class BaseAdminController : Controller
    {
        private readonly IQGameDbContext _context;

        public BaseAdminController(IQGameDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isRegistered = _context.GameUsers.Any(u => u.IdentityUserId == userId);

                if (!isRegistered)
                {
                    context.Result = RedirectToAction("Unregistered", "Home");
                }
            }
        }
    }
}
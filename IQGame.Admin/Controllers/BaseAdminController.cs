using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IQGame.Infrastructure.Persistence;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IQGame.Admin.Controllers
{
    [Authorize]
    public abstract class BaseAdminController : Controller
    {
        protected readonly IQGameDbContext _context;

        protected BaseAdminController(IQGameDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // ✅ Skip check if user is in Admin role
                if (User.IsInRole("Admin"))
                    return;

                var isRegistered = _context.GameUsers.Any(u => u.IdentityUserId == userId);

                if (!isRegistered)
                {
                    context.Result = RedirectToAction("Unregistered", "Home");
                }
            }
        }

    }
}
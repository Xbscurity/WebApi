using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace api.Authorization
{
    public class NotBannedHandler : AuthorizationHandler<NotBannedRequirement>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotBannedHandler(UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            NotBannedRequirement requirement)
        {
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return;

            var user = await _userManager.FindByIdAsync(userId);
            if (user is not null && !user.IsBanned)
            {
                context.Succeed(requirement);
            }
        }
    }
}

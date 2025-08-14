using api.Extensions;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace api.Authorization
{
    public class NotBannedHandler : AuthorizationHandler<NotBannedRequirement>
    {
        private readonly UserManager<AppUser> _userManager;

        public NotBannedHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            NotBannedRequirement requirement)
        {
            var userId = context.User.GetUserId();
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

using api.Extensions;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace api.Authorization
{
    /// <summary>
    /// Handles the <see cref="NotBannedRequirement"/> authorization requirement.
    /// Checks if the current user is not banned.
    /// </summary>
    public class NotBannedHandler : AuthorizationHandler<NotBannedRequirement>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<NotBannedHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotBannedHandler"/> class.
        /// </summary>
        /// <param name="userManager">The <see cref="UserManager{TUser}"/> instance for managing application users.</param>
        /// <param name="logger">The logger used to write access check details.</param>
        public NotBannedHandler(UserManager<AppUser> userManager, ILogger<NotBannedHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Makes a decision if the current user satisfies the <see cref="NotBannedRequirement"/>.
        /// </summary>
        /// <param name="context">The authorization context, which includes the current user principal.</param>
        /// <param name="requirement">The <see cref="NotBannedRequirement"/> to evaluate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method succeeds the requirement if:
        /// 1. The user is authenticated.
        /// 2. The user exists in the database.
        /// 3. The user is not banned.
        /// </remarks>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            NotBannedRequirement requirement)
        {
            var userId = context.User.GetUserId();

            var user = await _userManager.FindByIdAsync(userId!);

            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            _logger.LogDebug("Checking if user is banned: {UserId} is {BanStatus}", userId, user!.IsBanned);

            if (user is not null && !user.IsBanned)
            {
                context.Succeed(requirement);
            }
        }
    }
}

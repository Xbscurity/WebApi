using api.Services.User;
using Microsoft.AspNetCore.Authorization;

namespace api.Authorization
{
    /// <summary>
    /// Authorization handler that evaluates whether the current user is banned.
    /// </summary>
    /// <remarks>
    /// This handler is executed when the <see cref="NotBannedRequirement"/> policy is applied.
    /// </remarks>
    public class NotBannedHandler : AuthorizationHandler<NotBannedRequirement>
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotBannedHandler"/> class.
        /// </summary>
        /// <param name="userService">Service used to check user ban status.</param>
        public NotBannedHandler(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Evaluates whether the current user satisfies the <c>NotBanned</c> requirement.
        /// </summary>
        /// <param name="context">Authorization context containing the user principal.</param>
        /// <param name="requirement">The requirement being evaluated.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            NotBannedRequirement requirement)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return;
            }

            if (!await _userService.IsBannedAsync())
            {
                context.Succeed(requirement);
            }
        }
    }
}
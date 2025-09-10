using api.Extensions;
using api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace api.Middlewares
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
        private readonly ILogger<CustomAuthorizationMiddlewareResultHandler> _logger;

        public CustomAuthorizationMiddlewareResultHandler(ILogger<CustomAuthorizationMiddlewareResultHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                var requirements = string.Join(", ", policy.Requirements.Select(r => r.GetType().Name));
                _logger.LogWarning("Access forbidden for user {UserId}. Requirements not met: {@Requirements}.", context.User.GetUserId(), requirements);
                await context.Response.WriteAsJsonAsync(ApiResponse.Forbidden<object>());
                return;
            }

            if (authorizeResult.Challenged)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                _logger.LogDebug("Unauthorized access attempt");
                await context.Response.WriteAsJsonAsync(ApiResponse.Unauthorized<object>());
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}

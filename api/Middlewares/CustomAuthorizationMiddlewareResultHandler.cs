using api.Extensions;
using api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace api.Middlewares
{
    /// <summary>
    /// Custom authorization middleware result handler.
    /// </summary>
    /// <remarks>
    /// Intercepts authorization results to return standardized <see cref="ApiResponse"/> objects
    /// for forbidden (<c>403</c>) and unauthorized (<c>401</c>) requests, while delegating
    /// successful or default cases to the built-in handler.
    /// </remarks>
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
        private readonly ILogger<CustomAuthorizationMiddlewareResultHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthorizationMiddlewareResultHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger used to record authorization events.</param>
        public CustomAuthorizationMiddlewareResultHandler(ILogger<CustomAuthorizationMiddlewareResultHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the result of an authorization check.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="policy">The <see cref="AuthorizationPolicy"/> being applied.</param>
        /// <param name="authorizeResult">The <see cref="PolicyAuthorizationResult"/>
        /// produced by the authorization middleware.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>- Returns <see cref="StatusCodes.Status403Forbidden"/> with a standardized <see cref="ApiResponse"/>
        /// if the user is authenticated but lacks required permissions.</para>
        /// - Returns <see cref="StatusCodes.Status401Unauthorized"/> with a standardized <see cref="ApiResponse"/>
        /// if the user is not authenticated.
        /// <para>- Delegates other cases to the default <see cref="AuthorizationMiddlewareResultHandler"/>.</para>
        /// </remarks>
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
                _logger.LogWarning(
                    "Access forbidden for user {UserId}. Requirements not met: {@Requirements}.",
                    context.User.GetUserId(), requirements);
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

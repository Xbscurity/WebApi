using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace api.Middlewares
{
    /// <summary>
    /// Provides custom handling for authorization middleware results.
    /// </summary>
    /// <remarks>
    /// This handler customizes HTTP responses for authentication
    /// and authorization failures by returning standardized
    /// <c>ProblemDetails</c> responses for:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// HTTP 401 Unauthorized responses when authentication is required.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// HTTP 403 Forbidden responses when access is denied.
    /// </description>
    /// </item>
    /// </list>
    /// All other authorization results are delegated to the default
    /// <see cref="AuthorizationMiddlewareResultHandler"/> implementation.
    /// </remarks>
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CustomAuthorizationMiddlewareResultHandler"/> class.
        /// </summary>
        public CustomAuthorizationMiddlewareResultHandler()
        {
        }

        /// <summary>
        /// Handles the authorization result for the current HTTP request.
        /// </summary>
        /// <param name="next">
        /// The delegate representing the next middleware in the pipeline.
        /// </param>
        /// <param name="context">
        /// The current HTTP request context.
        /// </param>
        /// <param name="policy">
        /// The authorization policy applied to the request.
        /// </param>
        /// <param name="authorizeResult">
        /// The result of the authorization evaluation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous authorization handling operation.
        /// </returns>
        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Challenged)
            {
                await Results.Problem(
                    detail: "Authentication required.",
                    statusCode: StatusCodes.Status401Unauthorized)
                    .ExecuteAsync(context);
                return;
            }

            if (authorizeResult.Forbidden)
            {
                await Results.Problem(
                    detail: "You do not have permission to perform this action.",
                    statusCode: StatusCodes.Status403Forbidden)
                    .ExecuteAsync(context);
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}

using api.Extensions;
using Serilog.Context;

namespace api.Middlewares
{
    /// <summary>
    /// Middleware that enriches structured logs with request-related context data.
    /// </summary>
    public class LogEnrichmentMiddleware : IMiddleware
    {
        /// <summary>
        /// Invokes the middleware and enriches the logging context
        /// with request-specific metadata.
        /// </summary>
        /// <param name="context">
        /// The current HTTP request context.
        /// </param>
        /// <param name="next">
        /// The delegate representing the next middleware in the pipeline.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous middleware operation.
        /// </returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var userId = context.User.GetUserId() ?? "Anonymous";
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = context.Request.Headers.UserAgent.ToString();

            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("ClientIp", clientIp))
            using (LogContext.PushProperty("UserAgent", userAgent))
            {
                await next(context);
            }
        }
    }
}
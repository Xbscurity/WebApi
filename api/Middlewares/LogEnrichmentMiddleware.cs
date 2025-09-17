using api.Extensions;

namespace api.Middlewares
{
    /// <summary>
    /// Middleware for enriching Serilog log events with additional contextual information.
    /// </summary>
    /// <remarks>
    /// This middleware adds the following properties to the Serilog log context
    /// for the duration of each request:
    /// <list type="bullet">
    /// <item>
    /// <description><c>UserId</c> — the ID of the authenticated user, or <c>"Anonymous"</c> if not authenticated.</description>
    /// </item>
    /// <item>
    /// <description><c>ClientIp</c> — the remote IP address of the client making the request.</description>
    /// </item>
    /// </list>
    /// These properties will automatically appear in all Serilog logs generated
    /// during the request processing.
    /// </remarks>
    public class LogEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEnrichmentMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware delegate in the HTTP request pipeline.</param>
        public LogEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware to enrich the Serilog log context with <c>UserId</c> and <c>ClientIp</c>.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context)
        {
            var userName = context.User.GetUserId() ?? "Anonymous";

            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            using (Serilog.Context.LogContext.PushProperty("UserId", userName))
            using (Serilog.Context.LogContext.PushProperty("ClientIp", clientIp))
            {
                await _next(context);
            }
        }
    }
}

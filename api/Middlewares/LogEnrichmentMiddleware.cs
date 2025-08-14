using api.Extensions;

namespace api.Middlewares
{
    public class LogEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;

        public LogEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

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

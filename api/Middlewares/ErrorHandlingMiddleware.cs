using api.Responses;

namespace api.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var apiResponse = new ApiResponse
                {
                    Error = new ApiError
                    {
                        Code = "INTERNAL_ERROR",
                        Message = "Server error occurred",
                        Data = _env.IsDevelopment() ? ex.ToString() : null,
                    },
                };
                await context.Response.WriteAsJsonAsync(apiResponse);
            }
        }
    }
}

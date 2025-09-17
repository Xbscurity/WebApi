using api.Responses;

namespace api.Middlewares
{
    /// <summary>
    /// Middleware for centralized error handling.
    /// Captures unhandled exceptions, logs them, and returns a standardized API response.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next delegate in the middleware pipeline.</param>
        /// <param name="logger">The logger used to log unhandled exceptions.</param>
        /// <param name="env">The hosting environment, used to determine whether to include detailed error information.</param>
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Executes the middleware logic.
        /// Wraps request execution in a try/catch block to handle unhandled exceptions.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

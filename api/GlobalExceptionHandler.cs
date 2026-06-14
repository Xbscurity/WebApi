using Microsoft.AspNetCore.Diagnostics;

namespace api
{
    /// <summary>
    /// Global exception handler that converts unhandled exceptions into standardized HTTP problem responses.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
        /// </summary>
        /// <param name="logger">Logger used to record exceptions.</param>
        /// <param name="env">Provides information about the current hosting environment.</param>
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Attempts to handle an unhandled exception and write a standardized error response.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="cancellationToken">A token to monitor for request cancellation.</param>
        /// <returns>
        /// <c>true</c> if the exception was handled and a response was written; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Returns a <c>500 Internal Server Error</c> response using <c>ProblemDetails</c>.
        /// In development, includes exception details and stack trace in the response.
        /// </remarks>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception");

            Dictionary<string, object?>? extensions = new()
            {
                ["errorCode"] = "INTERNAL_ERROR",
            };

            if (_env.IsDevelopment())
            {
                extensions.Add("exception", exception.GetType().Name);
                extensions.Add("stackTrace", exception.StackTrace);
            }

            await Results.Problem(
                statusCode: 500,
                detail: _env.IsDevelopment() ? exception.Message : null,
                title: "Internal server error",
                extensions: extensions)
                .ExecuteAsync(context);

            return true;
        }
    }
}
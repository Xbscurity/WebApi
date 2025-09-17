using api.Constants;
using api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Filters
{
    /// <summary>
    /// A result filter that maps <see cref="ApiResponse"/> error codes
    /// to corresponding HTTP status codes in the response.
    /// </summary>
    /// <remarks>
    /// This filter ensures consistent HTTP status codes are returned
    /// based on the <see cref="ErrorCodes"/> provided in the API response.
    /// If no error is present, a <c>200 OK</c> status is applied by default.
    /// </remarks>
    public class StatusCodeFilter : IResultFilter
    {
        /// <summary>
        /// Called before the action result is executed.
        /// Maps <see cref="ApiResponse.Error"/> codes to HTTP status codes.
        /// </summary>
        /// <param name="context">
        /// The result executing context, containing the action result
        /// and HTTP response information.
        /// </param>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is ApiResponse response)
            {
                context.HttpContext.Response.StatusCode = response.Error?.Code switch
                {
                    ErrorCodes.NotFound => StatusCodes.Status404NotFound,
                    ErrorCodes.ValidationError => StatusCodes.Status422UnprocessableEntity,
                    ErrorCodes.BadRequest => StatusCodes.Status400BadRequest,
                    ErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
                    _ => StatusCodes.Status200OK
                };
            }
        }

        /// <summary>
        /// Called after the action result has been executed.
        /// This implementation does nothing.
        /// </summary>
        /// <param name="context">The executed result context.</param>
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
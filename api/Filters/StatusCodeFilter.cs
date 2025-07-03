using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Filters
{
    public class StatusCodeFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is ApiResponse response)
            {
                context.HttpContext.Response.StatusCode = response.Error?.Code switch
                {
                    "NOT_FOUND" => StatusCodes.Status404NotFound, // (int)HttpStatusCode.NotFound,
                    "VALIDATION_ERROR" => StatusCodes.Status422UnprocessableEntity,
                    "BAD_REQUEST" => StatusCodes.Status400BadRequest,
                    "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
                    "FORBIDDEN" => StatusCodes.Status403Forbidden,
                    _ => StatusCodes.Status200OK // (int)HttpStatusCode.OK
                };
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
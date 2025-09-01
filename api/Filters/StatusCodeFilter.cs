using api.Constants;
using api.Responses;
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
                    ErrorCodes.NotFound => StatusCodes.Status404NotFound, // (int)HttpStatusCode.NoAccess,
                    ErrorCodes.ValidationError => StatusCodes.Status422UnprocessableEntity,
                    ErrorCodes.BadRequest => StatusCodes.Status400BadRequest,
                    ErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
                    _ => StatusCodes.Status200OK // (int)HttpStatusCode.OK
                };
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
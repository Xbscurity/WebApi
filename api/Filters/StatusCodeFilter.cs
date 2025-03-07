using System.Net;
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
                    "NOT_FOUND" => (int)HttpStatusCode.NotFound,
                    _ => (int)HttpStatusCode.OK
                };
            }
        }
        public void OnResultExecuted(ResultExecutedContext context)
        {

        }

    }
}
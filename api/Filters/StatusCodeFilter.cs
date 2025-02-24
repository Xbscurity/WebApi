using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
                    "NOT_FOUND" => 404,
                    _ => 200
                };
            }
        }
        public void OnResultExecuted(ResultExecutedContext context)
        {

        }

    }
}
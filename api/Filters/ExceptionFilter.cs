using api.Enums;
using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.Result = new ObjectResult(new ApiResponse
            {
                Error = new Error
                {
                    Code = ErrorCodes.InternalServerError,
                    Message = "Server error occured"
                }
            })
            { StatusCode = StatusCodes.Status500InternalServerError }; //(int?)HttpStatusCode.InternalServerError

            context.ExceptionHandled = true;
        }
    }
}
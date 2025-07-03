using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;

        public ExceptionFilter(IWebHostEnvironment env)
        {
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            context.Result = new ObjectResult(new ApiResponse
            {
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Server error occured",
                    Data = _env.IsDevelopment() ? context.Exception.ToString() : string.Empty,
                },
            })
            { StatusCode = StatusCodes.Status500InternalServerError }; // (int?)HttpStatusCode.InternalServerError

            context.ExceptionHandled = true;
        }
    }
}
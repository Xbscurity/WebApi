using api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace api.Filters
{
    /// <summary>
    /// An action filter that validates model state before executing an action.
    /// </summary>
    /// <remarks>
    /// If the model state is invalid, this filter constructs an <see cref="ApiResponse{T}"/>
    /// containing the validation errors and returns a 422 Unprocessable Entity response.
    /// </remarks>
    public class ValidationFilter : IActionFilter
    {
        /// <summary>
        /// Called before the action executes. Checks if the <see cref="ModelStateDictionary"/> is valid.
        /// </summary>
        /// <param name="context">The <see cref="ActionExecutingContext"/> for the action.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                var response = new ApiResponse<object>
                {
                    Error = new ApiError
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Validation failed",
                        Data = errors,
                    },
                };

                context.Result = new UnprocessableEntityObjectResult(response);
            }
        }

        /// <summary>
        /// Called after the action executes. This implementation does nothing.
        /// </summary>
        /// <param name="context">The <see cref="ActionExecutedContext"/> for the action.</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }

}

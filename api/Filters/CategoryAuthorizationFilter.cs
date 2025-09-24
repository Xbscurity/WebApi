using api.Constants;
using api.Responses;
using api.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Filters
{
    public class CategoryAuthorizationFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<CategoryAuthorizationFilter> _logger;
        private readonly string _policy;

        public CategoryAuthorizationFilter(
           IAuthorizationService authorizationService,
           ILogger<CategoryAuthorizationFilter> logger,
           string policy)
        {
            _authorizationService = authorizationService;
            _logger = logger;
            _policy = policy;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue("id", out var idValue) || !(idValue is int categoryId))
            {
                context.Result = new BadRequestObjectResult(ApiResponse.BadRequest<object>());
                return;
            }

            var categoryService = context.HttpContext.RequestServices.GetRequiredService<ICategoryService>();
            var category = await categoryService.GetByIdRawAsync(categoryId);

            if (category == null)
            {
                context.Result = new NotFoundObjectResult(ApiResponse.NotFound<object>($"Category with ID {categoryId} not found."));
                _logger.LogDebug("Category {CategoryId} not found.", categoryId);
                return;
            }

            var user = context.HttpContext.User;
            var authResult = await _authorizationService.AuthorizeAsync(user, category, _policy);

            if (!authResult.Succeeded)
            {
                _logger.LogWarning(
                    LoggingEvents.Categories.Common.NoAccess,
                    "Access denied to category {CategoryId}", categoryId);

                var forbidCommon = _policy == Policies.CategoryAccessNoGlobal;

                if (forbidCommon && category.AppUserId == null)
                {
                    context.Result = new ObjectResult(ApiResponse.Forbidden<object>("Cannot modify common categories."))
                    {
                        StatusCode = 403,
                    };
                    return;
                }

                context.Result = new ObjectResult(ApiResponse.Forbidden<object>("Forbidden access to category."))
                {
                    StatusCode = 403,
                };
                return;
            }

            await next();
        }
    }

    public class CategoryAuthorizationAttribute : TypeFilterAttribute
    {
        public CategoryAuthorizationAttribute(string policy)
            : base(typeof(CategoryAuthorizationFilter))
        {
            Arguments = new object[] { policy };
        }
    }
}

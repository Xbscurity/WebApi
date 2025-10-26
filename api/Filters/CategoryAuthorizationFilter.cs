using api.Constants;
using api.Dtos.Interfaces;
using api.Responses;
using api.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Filters
{
    /// <summary>
    /// An asynchronous action filter that performs authorization for category-related actions.
    /// It checks if the authenticated user has the necessary permissions to access a specific category
    /// based on a provided authorization policy.
    /// </summary>
    public class CategoryAuthorizationFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<CategoryAuthorizationFilter> _logger;
        private readonly ICategoryService _categoryService;
        private readonly string _parameterName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryAuthorizationFilter"/> class.
        /// </summary>
        /// <param name="authorizationService">The service for performing authorization checks.</param>
        /// <param name="logger">The logger for this filter.</param>
        /// <param name="categoryService">The service for managing categories.</param>
        /// <param name="parameterName">
        /// The name of the action method parameter that contains the category identifier.
        /// Defaults to <c>"id"</c>. This value is used to extract the category ID
        /// from the <see cref="ActionExecutingContext.ActionArguments"/> collection.
        /// </param>
        public CategoryAuthorizationFilter(
           IAuthorizationService authorizationService,
           ILogger<CategoryAuthorizationFilter> logger,
           ICategoryService categoryService,
           string parameterName = "id")
        {
            _authorizationService = authorizationService;
            _logger = logger;
            _categoryService = categoryService;
            _parameterName = parameterName;
        }

        /// <summary>
        /// Called before and after the action is executed. It validates the category ID, retrieves the category,
        /// and authorizes the user against the specified policy.
        /// </summary>
        /// <param name="context">The context for action execution.</param>
        /// <param name="next">The delegate to call to proceed to the next action filter or the action itself.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execution.</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue(_parameterName, out var parameterValue))
            {
                context.Result = new BadRequestObjectResult(ApiResponse.BadRequest<object>("Missing parameter."));
                return;
            }

            int categoryId;

            if (parameterValue is IHasCategoryId hasCategory)
            {
                categoryId = hasCategory.CategoryId;
            }
            else if (parameterValue is int id)
            {
                categoryId = id;
            }
            else
            {
                context.Result = new BadRequestObjectResult(ApiResponse.BadRequest<object>(
                    "Invalid category identifier parameter."));
                return;
            }

            var category = await _categoryService.GetByIdRawAsync(categoryId);
            if (category == null)
            {
                context.Result = new NotFoundObjectResult(ApiResponse.NotFound<object>($"Category with ID {categoryId} not found."));
                _logger.LogDebug("Category {CategoryId} not found.", categoryId);
                return;
            }

            var user = context.HttpContext.User;
            var authResult = await _authorizationService.AuthorizeAsync(user, category, Policies.CategoryAccess);

            if (!authResult.Succeeded)
            {
                _logger.LogWarning(
                    LoggingEvents.Categories.Common.NoAccess,
                    "Access denied to category {CategoryId}",
                    categoryId);
                context.Result = new ObjectResult(ApiResponse.Forbidden<object>("Forbidden access to category."))
                {
                    StatusCode = 403,
                };
                return;
            }

            await next();
        }
    }

    /// <summary>
    /// An attribute to apply the <see cref="CategoryAuthorizationFilter"/> to an action method.
    /// This provides a declarative way to require authorization based on a policy for category-related actions.
    /// </summary>
    public class CategoryAuthorizationAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryAuthorizationAttribute"/> class.
        /// <param name="parameterName">
        /// The name of the action method parameter that contains the category identifier.
        /// Defaults to <c>"id"</c>. This value is used to extract the category ID
        /// from the <see cref="ActionExecutingContext.ActionArguments"/> collection.
        /// </param>
        /// </summary>
        public CategoryAuthorizationAttribute(string parameterName = "id")
            : base(typeof(CategoryAuthorizationFilter))
        {
            Arguments = new object[] { parameterName };
        }
    }
}

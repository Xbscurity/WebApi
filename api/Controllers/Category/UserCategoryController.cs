using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.QueryObjects;
using api.Responses;
using api.Services.Categories;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Category
{
    /// <summary>
    /// Provides API endpoints for users to manage their own categories.
    /// </summary>
    [Authorize(Policy = Policies.UserNotBanned)]
    [ApiController]
    [Route("api/user/categories")]
    public class UserCategoryController : BaseCategoryController
    {
        private readonly CategorySortValidator _sortValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCategoryController"/> class.
        /// </summary>
        /// <param name="categoryService">The service for managing categories.</param>
        /// <param name="logger">The logger for user-specific category operations.</param>
        /// <param name="sortValidator">Validates sorting fields for category queries.</param>
        /// <param name="authorizationService">The service used to authorize category access.</param>
        public UserCategoryController(
            ICategoryService categoryService,
            ILogger<UserCategoryController> logger,
            CategorySortValidator sortValidator,
            IAuthorizationService authorizationService)
            : base(categoryService, logger, authorizationService)
        {
            _sortValidator = sortValidator;
        }

        /// <summary>
        /// Retrieves all categories for the currently authenticated user.
        /// </summary>
        /// <param name="queryObject">The pagination and sorting query parameters.</param>
        /// <param name="includeInactive">
        /// If <see langword="true"/>, includes inactive categories in the result.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of the user's categories.
        /// </returns>
        [HttpGet]
        public virtual async Task<ApiResponse<List<BaseCategoryOutputDto>>> GetAll(
            [FromQuery] PaginationQueryObject queryObject, [FromQuery] bool includeInactive = false)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                Logger.LogWarning(LoggingEvents.Categories.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<BaseCategoryOutputDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var userId = User.GetUserId();
            var categories = await CategoryService.GetAllForUserAsync(userId!, queryObject, includeInactive);

            Logger.LogInformation(
                LoggingEvents.Categories.User.GetAll,
                "Returning {Count} categories. Page={PageNumber}, Size={PageSize}, SortBy={SortBy}",
                categories.Data.Count,
                categories.Pagination.PageNumber,
                categories.Pagination.PageSize,
                queryObject.SortBy);
            return ApiResponse.Success(categories.Data, categories.Pagination);
        }

        /// <summary>
        /// Creates a new category for the currently authenticated user.
        /// </summary>
        /// <param name="categoryDto">The category creation data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the newly created category.
        /// </returns>
        [HttpPost]
        public async Task<ApiResponse<BaseCategoryOutputDto>> Create([FromBody] BaseCategoryUpdateInputDto categoryDto)
        {
            var userId = User.GetUserId();
            var result = await CategoryService.CreateForUserAsync(userId!, categoryDto);
            Logger.LogInformation(LoggingEvents.Categories.User.Created, "Created new transaction {categoryId}", result.Id);
            return ApiResponse.Success(result);
        }
    }
}

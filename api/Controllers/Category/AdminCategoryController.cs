using api.Constants;
using api.Dtos.Category;
using api.QueryObjects;
using api.Responses;
using api.Services.Categories;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Category
{
    /// <summary>
    /// Provides API endpoints for administrators to manage categories across all users.
    /// </summary>
    [Authorize(Policy = Policies.Admin)]
    [ApiController]
    [Route("api/admin/categories")]
    public class AdminCategoryController : BaseCategoryController
    {
        private readonly CategorySortValidator _sortValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminCategoryController"/> class.
        /// </summary>
        /// <param name="categoriesService">The service for managing categories.</param>
        /// <param name="logger">The logger for admin-specific category operations.</param>
        /// <param name="sortValidator">Validates sorting fields for category queries.</param>
        /// <param name="authorizationService">The service used to authorize category access.</param>
        public AdminCategoryController(
            ICategoryService categoriesService,
            ILogger<AdminCategoryController> logger,
            CategorySortValidator sortValidator,
            IAuthorizationService authorizationService)
            : base(categoriesService, logger, authorizationService)
        {
            _sortValidator = sortValidator;
        }

        /// <summary>
        /// Retrieves all categories with optional filtering by user.
        /// </summary>
        /// <param name="queryObject">The pagination and sorting query parameters.</param>
        /// <param name="userId">Optional user ID to filter categories by owner.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of categories.
        /// </returns>
        [HttpGet]
        public async Task<ApiResponse<List<BaseCategoryOutputDto>>> GetAll(
            [FromQuery] PaginationQueryObject queryObject,
            [FromQuery] string? userId = null)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                Logger.LogWarning(LoggingEvents.Categories.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy));
                return ApiResponse.BadRequest<List<BaseCategoryOutputDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy));
            }

            var categories = await CategoryService.GetAllForAdminAsync(queryObject, userId);
            Logger.LogInformation(
                LoggingEvents.Categories.Admin.GetAll,
                "Returning {Count} categories. , Page={PageNumber}, Size={PageSize}, SortBy={SortBy}, userId = {userId}",
                categories.Data.Count,
                categories.Pagination.PageNumber,
                categories.Pagination.PageSize,
                queryObject.SortBy,
                userId);
            return ApiResponse.Success(categories.Data, categories.Pagination);
        }

        /// <summary>
        /// Creates a new category on behalf of a user.
        /// </summary>
        /// <param name="categoryDto">The category creation data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the newly created category.
        /// </returns>
        [HttpPost]
        public async Task<ApiResponse<BaseCategoryOutputDto>> Create([FromBody] AdminCategoryCreateInputDto categoryDto)
        {
            var result = await CategoryService.CreateForAdminAsync(categoryDto);
            Logger.LogInformation(
                LoggingEvents.Categories.Admin.Created,
                "Created new category {categoryId} for user {UserId}",
                result.Id,
                result.AppUserId);
            return ApiResponse.Success(result);
        }
    }
}

using api.Constants;
using api.Dtos.Category;
using api.QueryObjects;
using api.Responses;
using api.Services.Categories;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    [ApiController]
    [Route("api/admin/categories")]
    public class AdminCategoryController : BaseCategoryController
    {

        private readonly CategorySortValidator _sortValidator;

        public AdminCategoryController(
            ICategoryService categoriesService,
            ILogger<AdminCategoryController> logger,
            CategorySortValidator sortValidator,
            IAuthorizationService authorizationService)
            : base(categoriesService, logger, authorizationService)
        {
            _sortValidator = sortValidator;
        }

        [HttpGet]
        public async Task<ApiResponse<List<BaseCategoryOutputDto>>> GetAll(
            [FromQuery] PaginationQueryObject queryObject,
            [FromQuery] string? userId = null)
        {

            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.SortInvalid, _sortValidator.GetErrorMessage(queryObject.SortBy));
                return ApiResponse.BadRequest<List<BaseCategoryOutputDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy));
            }

            var categories = await _categoryService.GetAllForAdminAsync(queryObject, userId);
            _logger.LogInformation(
                LoggingEvents.Categories.Admin.GetAll,
                "Returning {Count} categories. , Page={PageNumber}, Size={PageSize}, SortBy={SortBy}, userId = {userId}",
                categories.Data.Count,
                categories.Pagination.PageNumber,
                categories.Pagination.PageSize,
                queryObject.SortBy,
                userId);
            return ApiResponse.Success(categories.Data, categories.Pagination);
        }

        [HttpPost]
        public async Task<ApiResponse<BaseCategoryOutputDto>> Create([FromBody] AdminCategoryInputDto categoryDto)
        {
            var result = await _categoryService.CreateForAdminAsync(categoryDto);
            _logger.LogInformation(
                LoggingEvents.Categories.Admin.Created,
                "Created new category {categoryId} for user {UserId}",
                result.Id, result.AppUserId);
            return ApiResponse.Success(result);
        }
    }
}

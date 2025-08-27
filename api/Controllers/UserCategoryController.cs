using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.QueryObjects;
using api.Responses;
using api.Services.Categories;
using api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(Policy = Policies.UserNotBanned)]
    [ApiController]
    [Route("api/user/categories")]
    public class UserCategoryController : BaseCategoryController
    {
        private readonly ILogger<UserCategoryController> _logger;
        private readonly CategorySortValidator _sortValidator;

        public UserCategoryController(
            ICategoryService categoryService,
            ILogger<UserCategoryController> logger,
            CategorySortValidator sortValidator)
            : base(categoryService, logger)
        {
            _logger = logger;
            _sortValidator = sortValidator;
        }

        [HttpGet]
        public virtual async Task<ApiResponse<List<BaseCategoryOutputDto>>> GetAll([FromQuery] PaginationQueryObject queryObject, [FromQuery] bool includeInactive = false)
        {
            if (!_sortValidator.IsValid(queryObject.SortBy))
            {
                _logger.LogWarning(_sortValidator.GetErrorMessage(queryObject.SortBy!));
                return ApiResponse.BadRequest<List<BaseCategoryOutputDto>>(_sortValidator.GetErrorMessage(queryObject.SortBy!));
            }

            var categories = await _categoryService.GetAllForUserAsync(User.ToCurrentUser(), queryObject, includeInactive);

            _logger.LogInformation(
                "Returning {Count} categories. Page={PageNumber}, Size={PageSize}, SortBy={SortBy}",
                categories.Data.Count,
                categories.Pagination.PageNumber,
                categories.Pagination.PageSize,
                queryObject.SortBy);
            return ApiResponse.Success(categories.Data, categories.Pagination);
        }

        [HttpPost]
        public async Task<ApiResponse<BaseCategoryOutputDto>> Create([FromBody] BaseCategoryInputDto categoryDto)
        {
            var userId = User.GetUserId();
            var result = await _categoryService.CreateForUserAsync(User.ToCurrentUser(), categoryDto);
            _logger.LogInformation("Created new transaction {categoryId}", result.Id);
            return ApiResponse.Success(result);
        }



        //[HttpGet("convert")]
        //public ApiResponse<TimeZoneRequest> GetTimeZoneInfo([FromQuery] TimeZoneRequest request)
        //{
        //    if (request.TimeZone is null)
        //    {
        //        return ApiResponse.NotFound<TimeZoneRequest>("Invalid or missing timezone.");
        //    }

        //    return ApiResponse.Success(request);
        //}
    }
}

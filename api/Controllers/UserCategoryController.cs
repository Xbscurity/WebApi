using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.QueryObjects;
using api.Responses;
using api.Services.Categories;
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

        public UserCategoryController(ICategoryService categoryService, ILogger<UserCategoryController> logger)
            : base(categoryService)
        {
            _logger = logger;
        }

        [HttpGet]
        public virtual async Task<ApiResponse<List<BaseCategoryOutputDto>>> GetAll([FromQuery] PaginationQueryObject queryObject, [FromQuery] bool includeInactive = false)
        {
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
            _logger.LogInformation(User.Identity.Name);
            _logger.LogWarning("On Category controller Get All method");
            var validSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "id", "name",
    };

            if (!string.IsNullOrWhiteSpace(queryObject.SortBy) && !validSortFields.Contains(queryObject.SortBy))
            {
                return ApiResponse.BadRequest<List<BaseCategoryOutputDto>>($"SortBy '{queryObject.SortBy}' is not a valid field.");
            }

            var categories = await _categoryService.GetAllForUserAsync(User, queryObject, includeInactive);
            return ApiResponse.Success(categories.Data, categories.Pagination);
        }

        [HttpPost]
        public async Task<ApiResponse<BaseCategoryOutputDto>> Create([FromBody] BaseCategoryInputDto categoryDto)
        {
            var userId = User.GetUserId();
            var result = await _categoryService.CreateForUserAsync(User, categoryDto);
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

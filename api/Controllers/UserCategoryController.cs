using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.Filters;
using api.Helpers;
using api.QueryObjects;
using api.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api.Controllers
{
    [Authorize(Policy = Policies.UserNotBanned)]
    [ServiceFilter(typeof(ExecutionTimeFilter))]
    [ApiController]
    [Route("api/user/categories")]
    public class UserCategoryController : BaseCategoryController
    {
        public UserCategoryController(ICategoryService categoryService)
            : base(categoryService)
        {
        }

        [HttpGet]
        public virtual async Task<ApiResponse<List<BaseCategoryOutputDto>>> GetAll([FromQuery] PaginationQueryObject queryObject, [FromQuery] bool includeInactive = false)
        {
            Log.Warning("On Category controller Get All method");
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

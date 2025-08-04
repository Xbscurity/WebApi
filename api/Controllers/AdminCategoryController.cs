using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.Filters;
using api.Models;
using api.QueryObjects;
using api.Responses;
using api.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    [ServiceFilter(typeof(ExecutionTimeFilter))]
    [ApiController]
    [Route("api/admin/categories")]
    public class AdminCategoryController : BaseCategoryController
    {

        public AdminCategoryController(ICategoryService categoriesService)
            : base(categoriesService)
        {
        }

        [HttpGet]
        public async Task<ApiResponse<List<Category>>> GetAll([FromQuery] PaginationQueryObject queryObject, [FromQuery] string? userId = null)
        {
            var validSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "id", "name",
    };

            if (!string.IsNullOrWhiteSpace(queryObject.SortBy) && !validSortFields.Contains(queryObject.SortBy))
            {
                return ApiResponse.BadRequest<List<Category>>($"SortBy '{queryObject.SortBy}' is not a valid field.");
            }

            var categories = await _categoryService.GetAllForAdminAsync(queryObject, userId);
            return ApiResponse.Success(categories.Data, categories.Pagination);
        }

        [HttpPost]
        public async Task<ApiResponse<Category>> Create([FromBody] AdminCategoryInputDto categoryDto)
        {
            var userId = User.GetUserId();
            var result = await _categoryService.CreateForAdminAsync(User, categoryDto);
            return ApiResponse.Success(result);
        }
    }
}

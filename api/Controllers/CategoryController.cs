using api.Dtos;
using api.Filters;
using api.Helpers;
using api.Models;
using api.QueryObjects;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{


    [ServiceFilter(typeof(ExecutionTimeFilter))]
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoriesService)
        {
            _categoryService = categoriesService;
        }
        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>A list of all categoeries</returns>
        [HttpGet]
        public async Task<ApiResponse<List<Category>>> GetAll([FromQuery] PaginationQueryObject queryObject)
        {
            var validSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "id", "name"
    };

            if (!string.IsNullOrWhiteSpace(queryObject.SortBy) && !validSortFields.Contains(queryObject.SortBy))
            {
                return ApiResponse.BadRequest<List<Category>>($"SortBy '{queryObject.SortBy}' is not a valid field.");
            }

            var categories = await _categoryService.GetAllAsync(queryObject);
            return ApiResponse.Success(categories.Data, categories.Pagination);
        }

        /// <summary>
        /// Get a specific category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to get.</param>
        /// <returns>The category with the specified ID.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<Category>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpGet("{id:int}")]
        public async Task<ApiResponse<Category>> GetById([FromRoute] int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category is null)
            {
                return ApiResponse.NotFound<Category>("Category not found");
            }
            return ApiResponse.Success(category);
        }
        /// <summary>
        /// Create a new category.
        /// </summary>
        /// <param name="categoryDto">The data for the new category.</param>
        /// <returns>The created category with its details</returns>
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<Category>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpPost]
        public async Task<ApiResponse<Category>> Create([FromBody] CategoryDto categoryDto)
        {
            var result = await _categoryService.CreateAsync(categoryDto);
            return ApiResponse.Success(result);
        }
        /// <summary>
        /// Update an existing category.
        /// </summary>
        /// <param name="id">The Id of the category to update.</param>
        /// <param name="categoryDto">The updated category data.</param>
        /// <returns>No content if the update is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpPut("{id:int}")]
        public async Task<ApiResponse<Category>> Update([FromRoute] int id, [FromBody] CategoryDto categoryDto)
        {
            var result = await _categoryService.UpdateAsync(id, categoryDto);
            if (result is null)
            {
                return ApiResponse.NotFound<Category>("Category not found");
            }
            return ApiResponse.Success(result);
        }
        /// <summary>
        /// Delete an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>No content if the delete is successful.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<Category>))]
        [HttpDelete("{id:int}")]

        public async Task<ApiResponse<bool>> Delete([FromRoute] int id)
        {

            var result = await _categoryService.DeleteAsync(id);
            if (result is false)
            {
                return ApiResponse.NotFound<bool>("Category not found");
            }
            return ApiResponse.Success(true);
        }
        [HttpGet("convert")]
        public ApiResponse<TimeZoneRequest> GetTimeZoneInfo([FromQuery] TimeZoneRequest request)
        {
            if (request.TimeZone is null)
            {
                return ApiResponse.NotFound<TimeZoneRequest>("Invalid or missing timezone.");
            }

            return ApiResponse.Success(request);
        }
    }
}

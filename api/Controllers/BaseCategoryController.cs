using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.Responses;
using api.Services.Categories;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public abstract class BaseCategoryController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected readonly ICategoryService _categoryService;


        public BaseCategoryController(ICategoryService categoriesService, ILogger logger)
        {
            _categoryService = categoriesService;
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public virtual async Task<ApiResponse<BaseCategoryOutputDto>> GetById([FromRoute] int id)
        {
            var category = await _categoryService.GetByIdAsync(User.ToCurrentUser(), id);
            if (category is null)
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.NotFound, "Category not found. CategoryId: {CategoryId}", id);
                return ApiResponse.NotFound<BaseCategoryOutputDto>("Category not found");
            }

            _logger.LogInformation(LoggingEvents.Categories.Common.GetById, "Returning category. CategoryId: {CategoryId}", category.Id);
            return ApiResponse.Success(category);
        }

        [HttpDelete("{id:int}")]
        public virtual async Task<ApiResponse<bool>> Delete([FromRoute] int id)
        {
            var result = await _categoryService.DeleteAsync(User.ToCurrentUser(), id);
            if (result is false)
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.NotFound, "Category not found. CategoryId: {CategoryId}", id);
                return ApiResponse.NotFound<bool>("Category not found");
            }

            _logger.LogInformation(LoggingEvents.Categories.Common.Deleted, "Category deleted. CategoryId: {CategoryId}", id);
            return ApiResponse.Success(true);
        }

        [HttpPut("{id:int}")]
        public virtual async Task<ApiResponse<BaseCategoryOutputDto>> Update([FromRoute] int id, [FromBody] BaseCategoryInputDto categoryDto)
        {
            var result = await _categoryService.UpdateAsync(User.ToCurrentUser(), id, categoryDto);
            if (result is null)
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.NotFound, "Category not found. CategoryId: {CategoryId}", id);
                return ApiResponse.NotFound<BaseCategoryOutputDto>("Category not found");
            }
            _logger.LogInformation(LoggingEvents.Categories.Common.Updated, "Category updated successfully. CategoryId: {CategoryId}", id);
            return ApiResponse.Success(result);
        }

        [HttpPatch("{id:int}/toggle-active")]
        public async Task<ApiResponse<bool>> ToggleActive([FromRoute] int id)
        {
            var success = await _categoryService.ToggleActiveAsync(User.ToCurrentUser(), id);
            if (!success)
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.NotFound, "Category not found. CategoryId: {CategoryId}", id);
                return ApiResponse.NotFound<bool>("Category not found");
            }
            _logger.LogInformation(LoggingEvents.Categories.Common.Toggled, "Category toggled successfully. CategoryId: {CategoryId}", id);
            return ApiResponse.Success(success);
        }
    }
}

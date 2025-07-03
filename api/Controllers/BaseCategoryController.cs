using api.Dtos.Category;
using api.Helpers;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public abstract class BaseCategoryController : ControllerBase
    {
        protected readonly ICategoryService _categoryService;

        public BaseCategoryController(ICategoryService categoriesService)
        {
            _categoryService = categoriesService;
        }

        [HttpGet("{id:int}")]
        public virtual async Task<ApiResponse<BaseCategoryOutputDto>> GetById([FromRoute] int id)
        {
            var category = await _categoryService.GetByIdAsync(User, id);
            if (category is null)
            {
                return ApiResponse.NotFound<BaseCategoryOutputDto>("Category not found");
            }

            return ApiResponse.Success(category);
        }

        [HttpDelete("{id:int}")]
        public virtual async Task<ApiResponse<bool>> Delete([FromRoute] int id)
        {
            var result = await _categoryService.DeleteAsync(User, id);
            if (result is false)
            {
                return ApiResponse.NotFound<bool>("Category not found");
            }

            return ApiResponse.Success(true);
        }

        [HttpPut("{id:int}")]
        public virtual async Task<ApiResponse<BaseCategoryOutputDto>> Update([FromRoute] int id, [FromBody] BaseCategoryInputDto categoryDto)
        {
            var result = await _categoryService.UpdateAsync(User, id, categoryDto);
            if (result is null)
            {
                return ApiResponse.NotFound<BaseCategoryOutputDto>("Category not found");
            }

            return ApiResponse.Success(result);
        }

        [HttpPatch("{id:int}/toggle-active")]
        public async Task<ApiResponse<bool>> ToggleActive([FromRoute] int id)
        {
            var success = await _categoryService.ToggleActiveAsync(User, id);
            if (!success)
            {
                return ApiResponse.NotFound<bool>("Category not found or access denied");
            }

            return ApiResponse.Success(success);
        }
    }
}

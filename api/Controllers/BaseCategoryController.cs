using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.Models;
using api.Responses;
using api.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public abstract class BaseCategoryController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected readonly ICategoryService _categoryService;

        private readonly IAuthorizationService _authorizationService;

        public BaseCategoryController(
            ICategoryService categoriesService,
            ILogger logger,
            IAuthorizationService authorizationService)
        {
            _categoryService = categoriesService;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        [HttpGet("{id:int}")]
        public virtual Task<ApiResponse<BaseCategoryOutputDto>> GetById([FromRoute] int id) =>
    HandleCategoryAsync(id, Policies.CategoryAccessGlobal,
        c => Task.FromResult<BaseCategoryOutputDto?>(c.ToOutputDto()));

        [HttpDelete("{id:int}")]
        public virtual Task<ApiResponse<bool>> Delete([FromRoute] int id) =>
            HandleCategoryAsync(id, Policies.CategoryAccessNoGlobal,
                async c => await _categoryService.DeleteAsync(id),
                forbidCommon: true);

        [HttpPut("{id:int}")]
        public virtual Task<ApiResponse<BaseCategoryOutputDto>> Update(
            [FromRoute] int id, [FromBody] BaseCategoryInputDto categoryDto) =>
            HandleCategoryAsync(id, Policies.CategoryAccessNoGlobal,
                async c => await _categoryService.UpdateAsync(id, categoryDto),
                forbidCommon: true);

        [HttpPatch("{id:int}/toggle-active")]
        public Task<ApiResponse<bool>> ToggleActive([FromRoute] int id) =>
            HandleCategoryAsync(id, Policies.CategoryAccessGlobal,
                async c => await _categoryService.ToggleActiveAsync(id),
                forbidCommon: true);

        private async Task<ApiResponse<T>> HandleCategoryAsync<T>(
    int id,
    string policy,
    Func<Category, Task<T?>> action,
    bool forbidCommon = false)
        {
            var category = await _categoryService.GetByIdRawAsync(id);
            if (category is null)
            {
                _logger.LogDebug("Category {CategoryId} not found", id);
                return ApiResponse.NotFound<T>("Category not found");
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, category, policy);
            if (!authResult.Succeeded)
            {
                _logger.LogWarning(
                    LoggingEvents.Categories.Common.NoAccess,
                    "Access denied to category {CategoryId}", id);

                if (forbidCommon && category.AppUserId == null)
                {
                    return ApiResponse.NotFound<T>("Cannot modify common categories");
                }

                return ApiResponse.NotFound<T>("Category not found");
            }

            var result = await action(category);
            return ApiResponse.Success(result!);
        }
    }
}

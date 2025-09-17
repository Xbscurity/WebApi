using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.Responses;
using api.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Category
{
    /// <summary>
    /// Provides a base controller for working with categories.
    /// Contains shared logic for retrieving, updating, deleting, and toggling categories.
    /// </summary>
    public abstract class BaseCategoryController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected readonly ICategoryService _categoryService;

        private readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCategoryController"/> class.
        /// </summary>
        /// <param name="categoriesService">The service for managing categories.</param>
        /// <param name="logger">The logger for recording category-related events.</param>
        /// <param name="authorizationService">The service used to authorize category access.</param>
        public BaseCategoryController(
            ICategoryService categoriesService,
            ILogger logger,
            IAuthorizationService authorizationService)
        {
            _categoryService = categoriesService;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Retrieves a category by its unique identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>An <see cref="ApiResponse{T}"/> containing the category if found.</returns>
        [HttpGet("{id:int}")]
        public virtual Task<ApiResponse<BaseCategoryOutputDto>> GetById([FromRoute] int id) =>
    HandleCategoryAsync(
        id,
        Policies.CategoryAccessGlobal,
        c => Task.FromResult<BaseCategoryOutputDto?>(c.ToOutputDto()));

        /// <summary>
        /// Deletes a category by its identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the deletion was successful.
        /// </returns>
        [HttpDelete("{id:int}")]
        public virtual Task<ApiResponse<bool>> Delete([FromRoute] int id) =>
            HandleCategoryAsync(id,
                Policies.CategoryAccessNoGlobal,
                async c => await _categoryService.DeleteAsync(id),
                forbidCommon: true);

        /// <summary>
        /// Updates the specified category.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <param name="categoryDto">The updated category data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the updated category.
        /// </returns>
        [HttpPut("{id:int}")]
        public virtual Task<ApiResponse<BaseCategoryOutputDto>> Update(
            [FromRoute] int id, [FromBody] BaseCategoryUpdateInputDto categoryDto) =>
            HandleCategoryAsync(id,
                Policies.CategoryAccessNoGlobal,
                async c => await _categoryService.UpdateAsync(id, categoryDto),
                forbidCommon: true);

        /// <summary>
        /// Toggles the active state of a category.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the toggle was successful.
        /// </returns>
        [HttpPatch("{id:int}/toggle-active")]
        public Task<ApiResponse<bool>> ToggleActive([FromRoute] int id) =>
            HandleCategoryAsync(id,
                Policies.CategoryAccessGlobal,
                async c => await _categoryService.ToggleActiveAsync(id),
                forbidCommon: true);

        private async Task<ApiResponse<T>> HandleCategoryAsync<T>(
    int id,
    string policy,
    Func<Models.Category, Task<T?>> action,
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

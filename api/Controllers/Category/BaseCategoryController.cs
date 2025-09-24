using api.Constants;
using api.Dtos.Category;
using api.Filters;
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
        }

        /// <summary>
        /// Retrieves a category by its unique identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>An <see cref="ApiResponse{T}"/> containing the category if found.</returns>
        [HttpGet("{id:int}")]
        [CategoryAuthorization(Policies.CategoryAccessGlobal)]
        public async virtual Task<ApiResponse<BaseCategoryOutputDto>> GetById([FromRoute] int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return ApiResponse.Success(result!);
        }

        /// <summary>
        /// Deletes a category by its identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the deletion was successful.
        /// </returns>
        [HttpDelete("{id:int}")]
        [CategoryAuthorization(Policies.CategoryAccessNoGlobal)]
        public async virtual Task<ApiResponse<bool>> Delete([FromRoute] int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return ApiResponse.Success(result!);
        }

        /// <summary>
        /// Updates the specified category.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <param name="categoryDto">The updated category data.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the updated category.
        /// </returns>
        [HttpPut("{id:int}")]
        [CategoryAuthorization(Policies.CategoryAccessNoGlobal)]
        public async virtual Task<ApiResponse<BaseCategoryOutputDto>> Update(
            [FromRoute] int id, [FromBody] BaseCategoryUpdateInputDto categoryDto)
        {
            var result = await _categoryService.UpdateAsync(id, categoryDto);
            return ApiResponse.Success(result!);
        }

        /// <summary>
        /// Toggles the active state of a category.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the toggle was successful.
        /// </returns>
        [HttpPatch("{id:int}/toggle-active")]
        [CategoryAuthorization(Policies.CategoryAccessGlobal)]
        public async Task<ApiResponse<bool>> ToggleActive([FromRoute] int id)
        {
            var result = await _categoryService.ToggleActiveAsync(id);
            return ApiResponse.Success(result!);
        }


    }
}

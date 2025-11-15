using api.Dtos.Category;
using api.Filters;
using api.Responses;
using api.Services.Categories;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Category
{
    /// <summary>
    /// Provides a base controller for working with categories.
    /// Contains shared logic for retrieving, updating, deleting, and toggling categories.
    /// </summary>
    public abstract class BaseCategoryController : ControllerBase
    {
        /// <summary>
        /// The service for managing categories.
        /// </summary>
        private readonly ICategoryService _categoryService;

        /// <summary>
        /// The logger for recording category-related events.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCategoryController"/> class.
        /// </summary>
        /// <param name="categoryService">The service for managing categories.</param>
        /// <param name="logger">The logger for recording category-related events.</param>
        public BaseCategoryController(
            ICategoryService categoryService,
            ILogger logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the service for managing categories.
        /// </summary>
        protected ICategoryService CategoryService => _categoryService;

        /// <summary>
        /// Gets the logger for recording category-related events.
        /// </summary>
        protected ILogger Logger => _logger;

        /// <summary>
        /// Retrieves a category by its unique identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>An <see cref="ApiResponse{T}"/> containing the category if found.</returns>
        [HttpGet("{id:int}")]
        [CategoryAuthorization]
        public async virtual Task<ApiResponse<BaseCategoryOutputDto>> GetById([FromRoute] int id)
        {
            var result = await CategoryService.GetByIdAsync(id);

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
        [CategoryAuthorization]
        public async virtual Task<ApiResponse<string>> Delete([FromRoute] int id)
        {
            await CategoryService.DeleteAsync(id);

            return ApiResponse.Success("Category has been deleted");
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
        [CategoryAuthorization]
        public async virtual Task<ApiResponse<BaseCategoryOutputDto>> Update(
            [FromRoute] int id, [FromBody] BaseCategoryUpdateInputDto categoryDto)
        {
            var result = await CategoryService.UpdateAsync(id, categoryDto);

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
        [CategoryAuthorization(includeInactive: true)]
        public async Task<ApiResponse<string>> ToggleActive([FromRoute] int id)
        {
            var result = await CategoryService.ToggleActiveAsync(id);

            if (result)
            {
                return ApiResponse.Success("Category activated");
            }

            return ApiResponse.Success("Category deactivated");
        }
    }
}

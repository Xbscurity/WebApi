using api.Constants;
using api.Dtos.Category;
using api.QueryObjects;
using api.Services.Categories;
using api.Services.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing categories used to group financial transactions.
    /// </summary>
    /// <remarks>
    /// All endpoints require authentication and are accessible only to users
    /// who satisfy the <c>NotBanned</c> authorization policy.
    /// </remarks>
    [Authorize(Policy = Policies.NotBanned)]
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryController"/> class.
        /// </summary>
        /// <param name="categoryService">
        /// The service responsible for category operations.
        /// </param>
        public CategoryController(
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Retrieves a paginated list of categories.
        /// </summary>
        /// <param name="query">
        /// The query parameters used for pagination, sorting, and filtering.
        /// </param>
        /// <returns>
        /// A paginated list of categories.
        /// </returns>
        /// <response code="200">
        /// Returns the paginated list of financial transactions.
        /// </response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PagedItems<CategoryOutputDto>>> GetAll(
            [FromQuery] EntityQuery query)
        {
            var categories = await _categoryService.GetAllAsync(query);

            return categories.ToActionResult(this);
        }

        /// <summary>
        /// Retrieves a category by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the category.</param>
        /// <returns>The requested category.</returns>
        /// <response code="200">
        /// Returns the requested category.
        /// </response>
        /// <response code="404">
        /// The specified category was not found.
        /// </response>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CategoryOutputDto>> GetById([FromRoute] Guid id)
        {
            var result = await _categoryService.GetByIdAsync(id);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="categoryDto">The category data used for creation.</param>
        /// <returns>The created category.</returns>
        /// <response code="201">
        /// The category was successfully created.
        /// </response>
        [HttpPost]
        public async Task<ActionResult<CategoryOutputDto>> Create(
            [FromBody] CategoryCreateInputDto categoryDto)
        {
            var result = await _categoryService.CreateAsync(categoryDto);

            if (result.IsError)
            {
                return result.ToActionResult(this);
            }

            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = result.Value.Id },
                value: result.Value);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">The identifier of the category to update.</param>
        /// <param name="categoryDto">The updated category data.</param>
        /// <returns>The updated category.</returns>
        /// <response code="200">
        /// The category was successfully updated.
        /// </response>
        /// <response code="404">
        /// The specified category was not found.
        /// </response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryOutputDto>> Update(
            [FromRoute] Guid id, [FromBody] CategoryUpdateInputDto categoryDto)
        {
            var result = await _categoryService.UpdateAsync(id, categoryDto);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Toggles the active status of a category.
        /// </summary>
        /// <param name="id">The identifier of the category.</param>
        /// <param name="isActive">The new active state.</param>
        /// <returns>The updated active status.</returns>
        /// <response code="200">
        /// The category active status was successfully updated.
        /// </response>
        /// <response code="404">
        /// The specified category was not found.
        /// </response>
        [HttpPatch("{id:guid}/toggle-active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ToggleActiveOutputDto>> ToggleActive([FromRoute] Guid id, [FromQuery] bool isActive)
        {
            var result = await _categoryService.SetActiveAsync(id, isActive);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="id">The identifier of the category to delete.</param>
        /// <returns>
        /// A <see cref="NoContentResult"/> when the category is successfully deleted.
        /// </returns>
        /// <response code="204">
        /// The category was successfully deleted.
        /// </response>
        /// <response code="404">
        /// The specified category was not found.
        /// </response>
        /// <response code="409">
        /// The category cannot be deleted because it is referenced by existing financial transactions.
        /// </response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return result.ToNoContentResult(this);
        }
    }
}

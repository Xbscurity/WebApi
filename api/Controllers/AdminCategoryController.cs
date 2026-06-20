using api.Constants;
using api.Dtos.Category;
using api.Queries;
using api.Services.Categories;
using api.Services.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    /// <summary>
    /// Provides administrative API endpoints for managing categories across all users.
    /// </summary>
    /// <remarks>
    /// All endpoints require administrator role.
    /// </remarks>
    [Authorize(Roles = Roles.Admin)]
    [ApiController]
    [Route("api/admin/categories")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AdminCategoryController : ControllerBase
    {
        private readonly IAdminCategoryService _adminCategoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminCategoryController"/> class.
        /// </summary>
        /// <param name="adminCategoryService">
        /// The service responsible for administrative category operations.
        /// </param>
        public AdminCategoryController(
            IAdminCategoryService adminCategoryService)
        {
            _adminCategoryService = adminCategoryService;
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
        /// Returns the paginated list of categories.
        /// </response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PagedItems<AdminCategoryOutputDto>>> GetAll(
            [FromQuery] AdminEntityQuery query)
        {
            var categories = await _adminCategoryService.GetAllAsync(query);

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
        public async Task<ActionResult<AdminCategoryOutputDto>> GetById([FromRoute] Guid id)
        {
            var result = await _adminCategoryService.GetByIdAsync(id);

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
        public async Task<ActionResult<AdminCategoryOutputDto>> Create(
            [FromBody] AdminCategoryCreateInputDto categoryDto)
        {
            var result = await _adminCategoryService.CreateAsync(categoryDto);

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
        public async Task<ActionResult<AdminCategoryOutputDto>> Update(
            [FromRoute] Guid id, [FromBody] CategoryUpdateInputDto categoryDto)
        {
            var result = await _adminCategoryService.UpdateAsync(id, categoryDto);

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
        [HttpPatch("{id:guid}/active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ToggleActiveOutputDto>> SetActive([FromRoute] Guid id, [FromQuery] bool isActive)
        {
            var result = await _adminCategoryService.SetActiveAsync(id, isActive);

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
            var result = await _adminCategoryService.DeleteAsync(id);
            return result.ToNoContentResult(this);
        }
    }
}

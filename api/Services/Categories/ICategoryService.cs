using api.Dtos.Category;
using api.Queries;
using api.Services.Shared;
using ErrorOr;

namespace api.Services.Categories
{
    /// <summary>
    /// Defines operations for managing categories.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves a paginated list of categories.
        /// </summary>
        /// <param name="query">
        /// The query parameters used for paging and sorting.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="PagedItems{T}"/>
        /// containing <see cref="CategoryOutputDto"/> if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<PagedItems<CategoryOutputDto>>> GetAllAsync(EntityQuery query);

        /// <summary>
        /// Retrieves a category by its identifier.
        /// </summary>
        /// <param name="id">
        /// The identifier of the category.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="CategoryOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<CategoryOutputDto>> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="input">
        /// The data required to create the category.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="CategoryOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<CategoryOutputDto>> CreateAsync(CategoryCreateInputDto input);

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">
        /// The identifier of the category to update.
        /// </param>
        /// <param name="input">
        /// The updated category data.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="CategoryOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<CategoryOutputDto>> UpdateAsync(Guid id, CategoryUpdateInputDto input);

        /// <summary>
        /// Sets the active status of a category.
        /// </summary>
        /// <param name="id">
        /// The identifier of the category.
        /// </param>
        /// <param name="isActive">
        /// The value indicating whether the category should be active.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="ToggleActiveOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<ToggleActiveOutputDto>> SetActiveAsync(Guid id, bool isActive);

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="id">
        /// The identifier of the category to delete.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="Deleted"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<Deleted>> DeleteAsync(Guid id);

        /// <summary>
        /// Creates the default set of categories for a user.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user for whom the categories should be created.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateInitialCategoriesForUserAsync(string userId);
    }
}
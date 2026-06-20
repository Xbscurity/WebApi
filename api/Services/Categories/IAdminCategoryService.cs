using api.Dtos.Category;
using api.Queries;
using api.Services.Shared;
using ErrorOr;

namespace api.Services.Categories
{
    /// <summary>
    /// Defines operations for managing categories as an administrator.
    /// </summary>
    public interface IAdminCategoryService
    {
        /// <summary>
        /// Retrieves a paginated list of categories for administrative purposes.
        /// </summary>
        /// <param name="query">
        /// The query parameters used for paging, sorting, and filtering.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a
        /// <see cref="PagedItems{T}"/> of <see cref="AdminCategoryOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<PagedItems<AdminCategoryOutputDto>>> GetAllAsync(
            AdminEntityQuery query);

        /// <summary>
        /// Retrieves a category by its identifier for administrative purposes.
        /// </summary>
        /// <param name="id">
        /// The identifier of the category.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing an
        /// <see cref="AdminCategoryOutputDto"/> if successful;
        /// otherwise, an error.
        /// </returns>
        Task<ErrorOr<AdminCategoryOutputDto>> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new category with administrative settings.
        /// </summary>
        /// <param name="input">
        /// The data required to create the category.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing an
        /// <see cref="AdminCategoryOutputDto"/> if successful;
        /// otherwise, an error.
        /// </returns>
        Task<ErrorOr<AdminCategoryOutputDto>> CreateAsync(
            AdminCategoryCreateInputDto input);

        /// <summary>
        /// Updates an existing category for administrative purposes.
        /// </summary>
        /// <param name="id">
        /// The identifier of the category to update.
        /// </param>
        /// <param name="input">
        /// The updated category data.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing an
        /// <see cref="AdminCategoryOutputDto"/> if successful;
        /// otherwise, an error.
        /// </returns>
        Task<ErrorOr<AdminCategoryOutputDto>> UpdateAsync(
                Guid id, CategoryUpdateInputDto input);

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
    }
}

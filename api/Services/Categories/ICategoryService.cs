using api.Data;
using api.Dtos.Category;
using api.Models;
using api.QueryObjects;
using api.Responses;

namespace api.Services.Categories
{
    /// <summary>
    /// Provides methods for managing categories, including retrieval, creation, updating, deletion,
    /// and toggling active status. Supports both user-specific and admin-level operations.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves a paginated list of categories available to a specific user.
        /// Includes the user’s own categories (active and optionally inactive).
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="queryObject">Pagination and sorting parameters.</param>
        /// <param name="includeInactive">Whether to include inactive categories owned by the user.</param>
        /// <returns>A task containing paginated category data.</returns>
        Task<PagedData<BaseCategoryOutputDto>> GetAllForUserAsync(string userId, PaginationQueryObject queryObject, bool includeInactive);

        /// <summary>
        /// Toggles the active status of a category by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>A task containing <see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</returns>
        Task<bool> ToggleActiveAsync(int id);

        /// <summary>
        /// Retrieves a paginated list of categories for administrative purposes.
        /// Can filter categories by user if a user ID is provided.
        /// </summary>
        /// <param name="queryObject">Pagination and sorting parameters.</param>
        /// <param name="userId">Optional user ID to filter categories.</param>
        /// <returns>A task containing paginated category data.</returns>
        Task<PagedData<BaseCategoryOutputDto>> GetAllForAdminAsync(PaginationQueryObject queryObject, string? userId);

        /// <summary>
        /// Retrieves a category by its identifier and maps it to an output DTO.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>A task containing the category DTO if found; otherwise, <see langword="null"/>.</returns>
        Task<BaseCategoryOutputDto?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a raw category entity by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>A task containing the category entity if found; otherwise, <see langword="null"/>.</returns>
        Task<Category?> GetByIdRawAsync(int id);

        /// <summary>
        /// Creates a new category for administrative purposes.
        /// </summary>
        /// <param name="category">The input data for creating a category, including user ID.</param>
        /// <returns>A task containing the created category DTO.</returns>
        Task<BaseCategoryOutputDto> CreateForAdminAsync(AdminCategoryCreateInputDto category);

        /// <summary>
        /// Creates a new category for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="categoryDto">The input data for creating a category.</param>
        /// <returns>A task containing the created category DTO.</returns>
        Task<BaseCategoryOutputDto> CreateForUserAsync(string userId, BaseCategoryUpdateInputDto categoryDto);

        /// <summary>
        /// Updates an existing category by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <param name="category">The updated category data.</param>
        /// <returns>A task containing the updated category DTO if successful; otherwise, <see langword="null"/>.</returns>
        Task<BaseCategoryOutputDto?> UpdateAsync(int id, BaseCategoryUpdateInputDto category);

        /// <summary>
        /// Deletes a category by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>A task containing <see langword="true"/> if the deletion succeeded; otherwise, <see langword="false"/>.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Creates a personalized set of default categories for a newly registered user.
        /// </summary>
        /// <remarks>
        /// This method retrieves the static templates from <see cref="DataSeeder.DefaultCategoryTemplates"/>,
        /// assigns the provided user ID to each category, and saves the new records to the database.
        /// </remarks>
        /// <param name="userId">The unique identifier (GUID or string) of the user for whom the categories are being created.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateInitialCategoriesForUserAsync(string userId);
    }
}
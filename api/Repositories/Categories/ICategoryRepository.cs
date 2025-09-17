using api.Models;

namespace api.Repositories.Categories
{
    /// <summary>
    /// Defines data access operations on <see cref="Category"/> entities.
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Retrieves all categories as a list.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the list of categories.</returns>
        Task<List<Category>> GetAllAsync();

        /// <summary>
        /// Returns a queryable collection of categories without tracking.
        /// </summary>
        /// <remarks>
        /// This method is useful when building complex LINQ queries that will be executed later.
        /// </remarks>
        /// <returns>
        /// A queryable sequence of <see cref="Category"/> entities.
        /// </returns>
        IQueryable<Category> GetQueryable();

        /// <summary>
        /// Retrieves a category by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the category if found;
        /// otherwise <see langword="null"/>.
        /// </returns>
        Task<Category?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new category in the database.
        /// </summary>
        /// <param name="category">The category entity to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateAsync(Category category);

        /// <summary>
        /// Updates an existing category in the database.
        /// </summary>
        /// <param name="category">The category entity with updated values.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(Category category);

        /// <summary>
        /// Deletes the specified category from the database.
        /// </summary>
        /// <param name="category">The category entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(Category category);
    }
}

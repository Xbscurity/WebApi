using api.Dtos.Category;
using api.Models;

namespace api.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Category"/> entities.
    /// </summary>
    public static class CategoryExtensions
    {
        /// <summary>
        /// Converts a <see cref="Category"/> entity
        /// into a <see cref="CategoryOutputDto"/>.
        /// </summary>
        /// <param name="category">
        /// The financial transaction entity to convert.
        /// </param>
        /// <returns>
        /// A DTO representation of the financial transaction.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is <see langword="null"/>.
        /// </exception>
        public static CategoryOutputDto ToOutputDto(this Category category)
        {
            ArgumentNullException.ThrowIfNull(category);

            return new CategoryOutputDto
            {
                Id = category.Id,
                Name = category.Name,
                AppUserId = category.AppUserId,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
            };
        }
    }
}
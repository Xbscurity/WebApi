using api.Dtos.Category;
using api.Models;

namespace api.Extensions
{
    /// <summary>
    /// Provides extension methods for mapping <see cref="Category"/> entities
    /// to their corresponding Data Transfer Objects (DTOs).
    /// </summary>
    public static class CategoryExtensions
    {
        /// <summary>
        /// Converts a <see cref="Category"/> entity to a <see cref="BaseCategoryOutputDto"/>.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> entity to convert.</param>
        /// <returns>
        /// A <see cref="BaseCategoryOutputDto"/> representation of the category,
        /// or <see langword="null"/> if the input is <see langword="null"/>.
        /// </returns>
        public static BaseCategoryOutputDto ToOutputDto(this Category category)
        {
            if (category is null)
            {
                return null;
            }

            return new BaseCategoryOutputDto
            {
                Id = category.Id,
                Name = category.Name,
                AppUserId = category.AppUserId,
                IsActive = category.IsActive,
            };
        }
    }
}

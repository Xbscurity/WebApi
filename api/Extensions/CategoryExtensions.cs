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
                return null!;
            }

            return new BaseCategoryOutputDto
            {
                Id = category.Id,
                Name = category.Name,
                AppUserId = category.AppUserId,
                IsActive = category.IsActive,
            };
        }

        /// <summary>
        /// Converts a <see cref="AdminCategoryCreateInputDto"/> dto to a <see cref="Category"/> model entity.
        /// </summary>
        /// <param name="dto">The <see cref="AdminCategoryCreateInputDto"/> dto to convert.</param>
        /// <returns>
        /// A <see cref="Category"/> entity model,
        /// or <see langword="null"/> if the input is <see langword="null"/>.
        /// </returns>
        public static Category ToModel(this AdminCategoryCreateInputDto dto)
        {
            if (dto is null)
            {
                return null!;
            }

            return new Category
            {
                AppUserId = dto.AppUserId,
                Name = dto.Name.Trim(),
            };
        }

        /// <summary>
        /// Converts a <see cref="BaseCategoryUpdateInputDto"/> dto to a <see cref="Category"/> model entity.
        /// </summary>
        /// <param name="dto">The <see cref="BaseCategoryUpdateInputDto"/> dto to convert.</param>
        /// <param name="userId"> <see cref="Category"/> entity will be created for this user id. </param>
        /// <returns>
        /// A <see cref="Category"/> entity model,
        /// or <see langword="null"/> if the input is <see langword="null"/>.
        /// </returns>
        public static Category ToModel(this BaseCategoryUpdateInputDto dto, string userId)
        {
            if (dto is null)
            {
                return null!;
            }

            return new Category
            {
                AppUserId = userId,
                Name = dto.Name.Trim(),
            };
        }
    }
}
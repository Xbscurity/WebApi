using api.Dtos.Category;
using api.Models;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Retrieves a specific <see cref="Category"/> entity by its identifier
    /// for a specific user and projects the result to <see cref="CategoryOutputDto"/>.
    /// </summary>
    public class CategoryByIdSpecification : Specification<Category, CategoryOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryByIdSpecification"/> class.
        /// </summary>
        /// <param name="id">The identifier of the category to retrieve.</param>
        /// <param name="userId">The identifier of the user who owns the category.</param>
        public CategoryByIdSpecification(Guid id, string userId)
        {
            Query
            .Where(c => c.AppUserId == userId)
            .Where(c => c.Id == id)
            .Select(c => new CategoryOutputDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
            });
        }
    }
}
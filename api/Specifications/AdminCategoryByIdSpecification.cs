using api.Dtos.Category;
using api.Models;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Retrieves a specific <see cref="Category"/> entity by its identifier
    /// and projects the result to <see cref="AdminCategoryOutputDto"/>
    /// for administrative purposes.
    /// </summary>
    public class AdminCategoryByIdSpecification : Specification<Category, AdminCategoryOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminCategoryByIdSpecification"/> class.
        /// </summary>
        /// <param name="id">The identifier of the category to retrieve.</param>
        public AdminCategoryByIdSpecification(Guid id)
        {
            Query
            .Where(t => t.Id == id)
            .Select(c => new AdminCategoryOutputDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                AppUserId = c.AppUserId,
            });
        }
    }
}
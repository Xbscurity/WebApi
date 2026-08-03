using api.Dtos.Category;
using api.Models;
using api.Queries;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Filters, sorts, and paginates <see cref="Category"/> entities for a specific user,
    /// projecting results to <see cref="CategoryOutputDto"/>.
    /// </summary>
    public class CategorySortedPagedSpecification : Specification<Category, CategoryOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategorySortedPagedSpecification"/> class.
        /// </summary>
        /// <param name="query">Query parameters for filtering, sorting, and pagination.</param>
        /// <param name="userId">The current user id context.</param>
        public CategorySortedPagedSpecification(EntityQuery query, string userId)
        {
            if (!query.IncludeInactive)
            {
                Query.Where(c => c.IsActive);
            }

            if (query.StartDate != null)
            {
                Query.Where(c => c.CreatedAt >= query.StartDate);
            }

            if (query.EndDate != null)
            {
                Query.Where(c => c.CreatedAt <= query.EndDate);
            }

            var sortBy = query.SortBy?.Trim().ToLowerInvariant();
            switch (sortBy)
            {
                case "name":
                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(c => c.Name)
                            .ThenByDescending(c => c.CreatedAt);
                    }
                    else
                    {
                        Query
                            .OrderBy(c => c.Name)
                            .ThenByDescending(c => c.CreatedAt);
                    }

                    break;
                case "isactive":
                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(c => c.IsActive)
                            .ThenByDescending(c => c.CreatedAt);
                    }
                    else
                    {
                        Query
                            .OrderBy(c => c.IsActive)
                            .ThenByDescending(c => c.CreatedAt);
                    }

                    break;
                default:

                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(c => c.CreatedAt);
                    }
                    else
                    {
                        Query.OrderBy(c => c.CreatedAt);
                    }

                    break;
            }

            Query
                .Where(c => c.AppUserId == userId)
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
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
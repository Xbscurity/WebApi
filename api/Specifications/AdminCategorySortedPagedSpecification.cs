using api.Dtos.Category;
using api.Models;
using api.Queries;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Filters, sorts, and paginates <see cref="Category"/> entities across all users,
    /// projecting results to <see cref="AdminCategoryOutputDto"/>.
    /// </summary>
    public class AdminCategorySortedPagedSpecification : Specification<Category, AdminCategoryOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminCategorySortedPagedSpecification"/> class.
        /// </summary>
        /// <param name="query">Admin query parameters for filtering, sorting, and pagination.</param>
        public AdminCategorySortedPagedSpecification(AdminEntityQuery query)
        {
            if (query.UserId != null)
            {
                Query.Where(c => c.AppUserId == query.UserId);
            }

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

            var sortBy = query.SortBy.Trim().ToLowerInvariant();
            switch (sortBy)
            {
                case "name":
                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(o => o.Name);
                    }
                    else
                    {
                        Query.OrderBy(o => o.Name);
                    }

                    break;
                default:

                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(o => o.Id);
                    }
                    else
                    {
                        Query.OrderBy(o => o.Id);
                    }

                    break;
            }

            Query
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);

            Query.Select(c => new AdminCategoryOutputDto
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

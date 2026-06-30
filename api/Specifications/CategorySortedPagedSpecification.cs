using api.Dtos.Category;
using api.Models;
using api.Providers.CurrentUser;
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
        /// <param name="currentUser">The current user context.</param>
        public CategorySortedPagedSpecification(EntityQuery query, ICurrentUser currentUser)
        {
            Query.Where(c => c.AppUserId == currentUser.UserId);

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
                case "isactive":
                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(o => o.IsActive);
                    }
                    else
                    {
                        Query.OrderBy(o => o.IsActive);
                    }

                    break;
                default:

                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(o => o.CreatedAt);
                    }
                    else
                    {
                        Query.OrderBy(o => o.CreatedAt);
                    }

                    break;
            }

            Query
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);

            Query.Select(c => new CategoryOutputDto
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

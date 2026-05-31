using api.Dtos.Category;
using api.Models;
using api.Providers.CurrentUser;
using api.QueryObjects;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Specification for retrieving paginated and sorted categories with filtering based on user context.
    /// </summary>
    public class CategorySortedPagedSpecification : Specification<Category, CategoryOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategorySortedPagedSpecification"/> class.
        /// </summary>
        /// <param name="query">Query parameters for filtering, sorting, and pagination.</param>
        /// <param name="currentUser">The current user context.</param>
        /// <remarks>
        /// Applies:
        /// <list type="bullet">
        /// <item><description>User-based filtering (admin can query by user, others limited to their own data)</description></item>
        /// <item><description>Optional filtering of inactive categories</description></item>
        /// <item><description>Sorting by name or identifier</description></item>
        /// <item><description>Pagination using page number and size</description></item>
        /// <item><description>Projection to <see cref="CategoryOutputDto"/></description></item>
        /// </list>
        /// </remarks>
        public CategorySortedPagedSpecification(EntityQuery query, ICurrentUser currentUser)
        {
            if (!currentUser.IsAdmin)
            {
                Query.Where(c => c.AppUserId == currentUser.UserId);
            }
            else if (!string.IsNullOrWhiteSpace(query.UserId))
            {
                Query.Where(c => c.AppUserId == query.UserId);
            }

            if (!query.IncludeInactive)
            {
                Query.Where(c => c.IsActive);
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

            Query.Select(c => new CategoryOutputDto
            {
                Id = c.Id,
                Name = c.Name,
                AppUserId = c.AppUserId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
            });
        }
    }
}

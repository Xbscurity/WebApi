using api.Dtos.FinancialTransaction;
using api.Models;
using api.Providers.CurrentUser;
using api.QueryObjects;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Specification for retrieving paginated and sorted financial transactions with filtering and projection.
    /// </summary>
    public class FinancialTransactionSortedPagedSpecification : Specification<FinancialTransaction, FinancialTransactionOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionSortedPagedSpecification"/> class.
        /// </summary>
        /// <param name="query">Query parameters for filtering, sorting, and pagination.</param>
        /// <param name="currentUser">The current user context.</param>
        /// <remarks>
        /// Applies:
        /// <list type="bullet">
        /// <item><description>User-based filtering (admin can query by user, others limited to their own data)</description></item>
        /// <item><description>Includes related category data</description></item>
        /// <item><description>Optional filtering of inactive categories</description></item>
        /// <item><description>Sorting by category, amount, date, or identifier</description></item>
        /// <item><description>Pagination using page number and size</description></item>
        /// <item><description>Projection to <see cref="FinancialTransactionOutputDto"/></description></item>
        /// </list>
        /// </remarks>
        public FinancialTransactionSortedPagedSpecification(EntityQuery query, ICurrentUser currentUser)
        {
            if (!currentUser.IsAdmin)
            {
                Query.Where(c => c.AppUserId == currentUser.UserId);
            }
            else if (!string.IsNullOrWhiteSpace(query.UserId))
            {
                Query.Where(c => c.AppUserId == query.UserId);
            }

            Query.Include(ft => ft.Category);

            if (!query.IncludeInactive)
            {
                Query.Where(c => c.Category.IsActive);
            }

            var sortBy = query.SortBy.Trim().ToLowerInvariant();

            switch (sortBy)
            {
                case "category":
                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(o => o.Category.Name);
                    }
                    else
                    {
                        Query.OrderBy(o => o.Category.Name);
                    }

                    break;
                case "amount":
                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(o => o.Amount);
                    }
                    else
                    {
                        Query.OrderBy(ft => ft.Amount);
                    }

                    break;

                case "date":
                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(ft => ft.CreatedAt);
                    }
                    else
                    {
                        Query.OrderBy(ft => ft.CreatedAt);
                    }

                    break;
                default:

                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(ft => ft.Id);
                    }
                    else
                    {
                        Query.OrderBy(ft => ft.Id);
                    }

                    break;
            }

            Query
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);

            Query.Select(ft => new FinancialTransactionOutputDto
            {
                Id = ft.Id,
                CategoryId = ft.CategoryId,
                Amount = ft.Amount,
                Type = ft.Type,
                Comment = ft.Comment,
                CreatedAt = ft.CreatedAt,
                UpdatedAt = ft.UpdatedAt,
                AppUserId = ft.AppUserId,
            });
        }
    }
}
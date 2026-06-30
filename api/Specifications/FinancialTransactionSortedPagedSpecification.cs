using api.Dtos.FinancialTransaction;
using api.Models;
using api.Providers.CurrentUser;
using api.Queries;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Filters, sorts, and paginates <see cref="FinancialTransaction"/> entities for a specific user,
    /// projecting results to <see cref="FinancialTransactionOutputDto"/>.
    /// </summary>
    public class FinancialTransactionSortedPagedSpecification : Specification<FinancialTransaction, FinancialTransactionOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionSortedPagedSpecification"/> class.
        /// </summary>
        /// <param name="query">Query parameters for filtering, sorting, and pagination.</param>
        /// <param name="currentUser">The current user context.</param>
        public FinancialTransactionSortedPagedSpecification(EntityQuery query, ICurrentUser currentUser)
        {
            Query.Where(c => c.AppUserId == currentUser.UserId);

            Query.Include(ft => ft.Category);

            if (query.StartDate != null)
            {
                Query.Where(c => c.CreatedAt >= query.StartDate);
            }

            if (query.EndDate != null)
            {
                Query.Where(c => c.CreatedAt <= query.EndDate);
            }

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
                default:

                    if (query.IsDescending)
                    {
                        Query.OrderByDescending(ft => ft.CreatedAt);
                    }
                    else
                    {
                        Query.OrderBy(ft => ft.CreatedAt);
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
            });
        }
    }
}
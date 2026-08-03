using api.Dtos.FinancialTransaction;
using api.Models;
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
        /// <param name="userId">The identifier of the user whose financial transactions are queried.</param>
        public FinancialTransactionSortedPagedSpecification(EntityQuery query, string userId)
        {
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

            var sortBy = query.SortBy?.Trim().ToLowerInvariant();

            switch (sortBy)
            {
                case "category":
                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(o => o.Category.Name)
                            .ThenByDescending(o => o.CreatedAt);
                    }
                    else
                    {
                        Query
                            .OrderBy(o => o.Category.Name)
                            .ThenByDescending(o => o.CreatedAt);
                    }

                    break;
                case "amount":
                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(o => o.Amount)
                            .ThenByDescending(o => o.CreatedAt);
                    }
                    else
                    {
                        Query
                            .OrderBy(ft => ft.Amount)
                            .ThenByDescending(o => o.CreatedAt);
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
                .Where(c => c.AppUserId == userId)
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
                .Select(ft => new FinancialTransactionOutputDto
                {
                    Id = ft.Id,
                    CategoryId = ft.CategoryId,
                    CategoryName = ft.Category.Name,
                    Amount = ft.Amount,
                    Type = ft.Type,
                    Comment = ft.Comment,
                    CreatedAt = ft.CreatedAt,
                    UpdatedAt = ft.UpdatedAt,
                });
        }
    }
}
using api.Dtos.FinancialTransaction;
using api.Models;
using api.Queries;
using Ardalis.Specification;

namespace api.Specifications
{
    /// <summary>
    /// Filters, sorts, and paginates <see cref="FinancialTransaction"/> entities across all users,
    /// projecting results to <see cref="AdminFinancialTransactionOutputDto"/>.
    /// </summary>
    public class AdminFinancialTransactionSortedPagedSpecification : Specification<FinancialTransaction, AdminFinancialTransactionOutputDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminFinancialTransactionSortedPagedSpecification"/> class.
        /// </summary>
        /// <param name="query">Admin query parameters for filtering, sorting, and pagination.</param>
        public AdminFinancialTransactionSortedPagedSpecification(AdminEntityQuery query)
        {
            if (query.UserId != null)
            {
                Query.Where(ft => ft.AppUserId == query.UserId);
            }

            if (query.StartDate != null)
            {
                Query.Where(ft => ft.CreatedAt >= query.StartDate);
            }

            if (query.EndDate != null)
            {
                Query.Where(ft => ft.CreatedAt <= query.EndDate);
            }

            if (!query.IncludeInactive)
            {
                Query.Where(ft => ft.Category.IsActive);
            }

            var sortBy = query.SortBy?.Trim().ToLowerInvariant();

            switch (sortBy)
            {
                case "category":
                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(ft => ft.Category.Name)
                            .ThenByDescending(ft => ft.CreatedAt);
                    }
                    else
                    {
                        Query
                            .OrderBy(ft => ft.Category.Name)
                            .ThenByDescending(ft => ft.CreatedAt);
                    }

                    break;

                case "amount":
                    if (query.IsDescending)
                    {
                        Query
                            .OrderByDescending(ft => ft.Amount)
                            .ThenByDescending(ft => ft.CreatedAt);
                    }
                    else
                    {
                        Query
                            .OrderBy(ft => ft.Amount)
                            .ThenByDescending(ft => ft.CreatedAt);
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
            .Take(query.Size)
            .Select(ft => new AdminFinancialTransactionOutputDto
            {
                Id = ft.Id,
                CategoryId = ft.CategoryId,
                Amount = ft.Amount,
                CategoryName = ft.Category.Name,
                Type = ft.Type,
                Comment = ft.Comment,
                CreatedAt = ft.CreatedAt,
                UpdatedAt = ft.UpdatedAt,
                AppUserId = ft.AppUserId,
            });
        }
    }
}
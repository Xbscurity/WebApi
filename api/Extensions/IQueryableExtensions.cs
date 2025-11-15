using api.Models;
using api.QueryObjects;

namespace api.Extensions
{
    /// <summary>
    /// Provides extension methods for applying sorting and filtering to <see cref="IQueryable{T}"/> sequences.
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Applies sorting to a query of <see cref="Category"/> entities based on the provided <see cref="PaginationQueryObject"/>.
        /// </summary>
        /// <param name="query">The queryable collection of <see cref="Category"/> entities.</param>
        /// <param name="queryObject">The pagination and sorting options.</param>
        /// <returns>
        /// An <see cref="IQueryable{T}"/> of <see cref="Category"/> entities ordered according to the specified criteria.
        /// Defaults to ordering by <c>Id</c> if no valid sort field is provided.
        /// </returns>
        public static IQueryable<Category> ApplySorting(
            this IQueryable<Category> query, PaginationQueryObject queryObject)
        {
            if (string.IsNullOrWhiteSpace(queryObject.SortBy))
            {
                return query.OrderBy(c => c.Id);
            }

            queryObject.SortBy = queryObject.SortBy.ToLowerInvariant();

            return queryObject.SortBy switch
            {
                "id" => queryObject.IsDescending ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
                "name" => queryObject.IsDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                _ => query.OrderBy(c => c.Id)
            };
        }

        /// <summary>
        /// Applies sorting to a query of <see cref="FinancialTransaction"/> entities based on the provided <see cref="PaginationQueryObject"/>.
        /// </summary>
        /// <param name="query">The queryable collection of <see cref="FinancialTransaction"/> entities.</param>
        /// <param name="queryObject">The pagination and sorting options.</param>
        /// <returns>
        /// An <see cref="IQueryable{T}"/> of <see cref="FinancialTransaction"/> entities ordered according to the specified criteria.
        /// Defaults to ordering by <c>Id</c> if no valid sort field is provided.
        /// </returns>
        public static IQueryable<FinancialTransaction> ApplySorting(
            this IQueryable<FinancialTransaction> query, PaginationQueryObject queryObject)
        {
            if (string.IsNullOrWhiteSpace(queryObject.SortBy))
            {
                return query.OrderBy(o => o.Id);
            }

            queryObject.SortBy = queryObject.SortBy.ToLowerInvariant();

            return queryObject.SortBy switch
            {
                "id" => queryObject.IsDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id),
                "category" => queryObject.IsDescending
                 ? query.OrderByDescending(o => o.Category != null ? o.Category.Name : null)
                 : query.OrderBy(o => o.Category != null ? o.Category.Name : null),
                "amount" => queryObject.IsDescending ? query.OrderByDescending(o => o.Amount) : query.OrderBy(o => o.Amount),
                "date" => queryObject.IsDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt),
                _ => query.OrderBy(o => o.Id)
            };
        }

        /// <summary>
        /// Applies filtering by date range to a query of <see cref="FinancialTransaction"/> entities.
        /// </summary>
        /// <param name="query">The queryable collection of <see cref="FinancialTransaction"/> entities.</param>
        /// <param name="dataRange">The report query object containing optional <c>StartDate</c> and <c>EndDate</c> filters.</param>
        /// <returns>
        /// An <see cref="IQueryable{T}"/> of <see cref="FinancialTransaction"/> entities
        /// filtered according to the provided date range.
        /// </returns>
        public static IQueryable<FinancialTransaction> ApplyFiltering(
            this IQueryable<FinancialTransaction> query, ReportQueryObject? dataRange)
        {
            if (dataRange?.StartDate != null)
            {
                query = query.Where(t => t.CreatedAt >= dataRange.StartDate.Value);
            }

            if (dataRange?.EndDate != null)
            {
                query = query.Where(t => t.CreatedAt <= dataRange.EndDate.Value);
            }

            return query;
        }
    }
}

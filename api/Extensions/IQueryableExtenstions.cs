using api.Models;
using api.QueryObjects;

namespace api.Extensions
{
    public static class IQueryableExtenstions
    {
        public static IQueryable<Category> ApplySorting(this IQueryable<Category> query, PaginationQueryObject queryObject)
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

        public static IQueryable<FinancialTransaction> ApplySorting(this IQueryable<FinancialTransaction> query, PaginationQueryObject queryObject)
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

        public static IQueryable<FinancialTransaction> ApplyFiltering(this IQueryable<FinancialTransaction> query, ReportQueryObject? dataRange)
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

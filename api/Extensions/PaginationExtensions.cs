using api.QueryObjects;
using api.Responses;
using api.Services.QueryModels;
using Microsoft.EntityFrameworkCore;

namespace api.Extensions
{
    public static class PaginationExtensions
    {
        public static async Task<PagedQuery<T>> ToPagedQueryAsync<T>(
       this IQueryable<T> query,
       PaginationQueryObject queryObject)
        {
            var totalItems = await query.CountAsync();
            var items = query
                .Skip((queryObject.Page - 1) * queryObject.Size)
                .Take(queryObject.Size);

            return new PagedQuery<T>
            {
                Query = items,
                Pagination = new Pagination
                {
                    PageNumber = queryObject.Page,
                    PageSize = queryObject.Size,
                    TotalItems = totalItems,
                },
            };
        }
    }
}

using api.QueryObjects;
using api.Responses;
using api.Services.QueryModels;
using Microsoft.EntityFrameworkCore;

namespace api.Extensions
{
    /// <summary>
    /// Provides extension methods for applying pagination to <see cref="IQueryable{T}"/> sequences.
    /// </summary>
    public static class PaginationExtensions
    {
        /// <summary>
        /// Converts a query into a paged query by applying <see cref="PaginationQueryObject"/> options.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the query.</typeparam>
        /// <param name="query">The queryable collection to paginate.</param>
        /// <param name="queryObject">The pagination parameters, such as page number and page size.</param>
        /// <returns>
        /// A <see cref="PagedQuery{T}"/> containing the paginated query
        /// and a <see cref="Pagination"/> object with metadata about the paging operation.
        /// </returns>
        /// <remarks>
        /// - Applies <c>Skip</c> and <c>Take</c> LINQ operators based on the page and size.
        /// <para>- Retrieves the total item count before pagination is applied.</para>
        /// </remarks>
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

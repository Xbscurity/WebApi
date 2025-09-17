using api.Responses;

namespace api.Services.QueryModels
{
    /// <summary>
    /// Defines the result of applying pagination to a query.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    public class PagedQuery<T>
    {
        /// <summary>
        /// Gets or sets the queryable sequence containing the paginated items.
        /// </summary>
        /// <remarks>
        /// This sequence already has pagination operators (<c>Skip</c>, <c>Take</c>) applied
        /// but can still be further composed before execution.
        /// </remarks>
        required public IQueryable<T> Query { get; set; }

        /// <summary>
        /// Gets or sets the pagination metadata, including page number, page size,
        /// and the total number of items before pagination was applied.
        /// </summary>
        required public Pagination Pagination { get; set; }
    }
}

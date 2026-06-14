namespace api.Queries
{
    /// <summary>
    /// Represents query parameters for retrieving paginated users collection.
    /// </summary>
    /// <remarks>
    /// Extends <see cref="PagedQuery"/> with sorting and filtering options.
    /// </remarks>
    public record UserManagementQuery : PagedQuery
    {
        /// <summary>
        /// Gets the field used for sorting.
        /// </summary>
        /// <value>
        /// The sort field name.
        /// Defaults to <c>"id"</c>.
        /// </value>
        public string SortBy { get; init; } = "id";

        /// <summary>
        /// Gets a value indicating whether sorting should be performed
        /// in descending order.
        /// </summary>
        /// <value>
        /// <see langword="true"/> for descending order;
        /// otherwise, <see langword="false"/>.
        /// Defaults to <see langword="false"/>.
        /// </value>
        public bool IsDescending { get; init; }
    }
}

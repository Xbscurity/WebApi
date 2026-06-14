namespace api.Queries
{
    /// <summary>
    /// Represents query parameters for retrieving paginated entity collections.
    /// </summary>
    /// <remarks>
    /// Extends <see cref="PagedQuery"/> with sorting and filtering options.
    /// </remarks>
    public record EntityQuery : PagedQuery
    {
        /// <summary>
        /// Gets a value indicating whether sorting should be performed
        /// in descending order.
        /// </summary>
        /// <value>
        /// <see langword="true"/> for descending order;
        /// otherwise, <see langword="false"/>.
        /// Defaults to <see langword="false"/>.
        /// </value>
        public bool IsDescending { get; init; } = false;

        /// <summary>
        /// Gets the field used for sorting.
        /// </summary>
        /// <value>
        /// The sort field name.
        /// Defaults to <c>"id"</c>.
        /// </value>
        public string SortBy { get; init; } = "id";

        /// <summary>
        /// Gets a value indicating whether inactive entities
        /// should be included in the result set.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to include inactive entities;
        /// otherwise, <see langword="false"/>.
        /// Defaults to <see langword="false"/>.
        /// </value>
        public bool IncludeInactive { get; init; } = false;

        /// <summary>
        /// Gets the start date used to filter entities.
        /// </summary>
        public DateTimeOffset? StartDate { get; init; }

        /// <summary>
        /// Gets the end date used to filter entities.
        /// </summary>
        public DateTimeOffset? EndDate { get; init; }
    }
}

namespace api.Services.Shared
{
    /// <summary>
    /// Represents a paginated collection of items together with pagination metadata.
    /// </summary>
    /// <typeparam name="T">
    /// The type of items contained in the collection.
    /// </typeparam>
    public record PagedItems<T>
    {
        /// <summary>
        /// Gets the items contained in the current page.
        /// </summary>
        required public IReadOnlyList<T> Items { get; init; }

        /// <summary>
        /// Gets pagination information associated with the current result set.
        /// </summary>
        required public Pagination Pagination { get; init; }
    }
}

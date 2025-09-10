namespace api.Responses
{
    /// <summary>
    /// Represents a paged collection of items returned by the API services.
    /// Used by controllers to construct <see cref="ApiResponse{T}"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of the items contained in the collection.</typeparam>
    public class PagedData<T>
    {
        /// <summary>
        /// Gets or sets the collection of items for the current page.
        /// </summary>
        public List<T> Data { get; set; } = new();

        /// <summary>
        /// Gets or sets pagination details such as total count and current page.
        /// </summary>
        public Pagination Pagination { get; set; } = new();
    }
}

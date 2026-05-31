namespace api.Services.Shared
{
    /// <summary>
    /// Represents pagination metadata for a paged result set.
    /// </summary>
    public record Pagination
    {
        /// <summary>
        /// Gets the current page number.
        /// </summary>
        /// <value>
        /// A one-based page index.
        /// </value>
        public int PageNumber { get; }

        /// <summary>
        /// Gets the maximum number of items per page.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Gets the total number of items across all pages.
        /// </summary>
        public int TotalItems { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pagination"/> class.
        /// </summary>
        /// <param name="pageNumber">
        /// The current page number. Must be greater than zero.
        /// </param>
        /// <param name="pageSize">
        /// The number of items per page. Must be greater than zero.
        /// </param>
        /// <param name="totalItems">
        /// The total number of items in the full result set. Cannot be negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when:
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pageNumber"/> is less than 1.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="pageSize"/> is less than 1.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="totalItems"/> is less than 0.</description>
        /// </item>
        /// </list>
        /// </exception>
        public Pagination(int pageNumber, int pageSize, int totalItems)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber),
                    pageNumber,
                    "Page number must be greater than 0");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize),
                    pageSize,
                    "Page size must be greater than 0");
            }

            if (totalItems < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalItems),
                    totalItems,
                    "Total items must be greater than or equal to 0");
            }

            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalItems = totalItems;
        }

        /// <summary>
        /// Gets the total number of available pages.
        /// </summary>
        public int TotalPages =>
            (TotalItems + PageSize - 1) / PageSize;

        /// <summary>
        /// Gets a value indicating whether a previous page exists.
        /// </summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// Gets a value indicating whether a next page exists.
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;
    }
}

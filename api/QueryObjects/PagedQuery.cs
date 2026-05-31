using System.ComponentModel.DataAnnotations;

namespace api.QueryObjects
{
    /// <summary>
    /// Represents common pagination query parameters.
    /// </summary>
    public record PagedQuery
    {
        /// <summary>
        /// Gets the page number to retrieve.
        /// </summary>
        /// <value>
        /// The page number. Must be greater than zero.
        /// Defaults to <c>1</c>.
        /// </value>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; init; } = 1;

        /// <summary>
        /// Gets the number of items to include per page.
        /// </summary>
        /// <value>
        /// The page size. Defaults to <c>10</c>.
        /// </value>
        [Range(1, 100, ErrorMessage = "Size must be between 1 and 100")]
        public int Size { get; init; } = 10;
    }
}

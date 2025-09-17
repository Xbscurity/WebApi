using System.ComponentModel.DataAnnotations;

namespace api.QueryObjects
{
    /// <summary>
    /// Defines query parameters for paginating, sorting, and ordering API results.
    /// </summary>
    public record PaginationQueryObject
    {
        /// <summary>
        /// Gets or sets the current page number (1-based index).
        /// <para>Must be greater than or equal to <see langword="1"/>. Defaults to <see langword="1"/>.</para>
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of items per page.
        /// <para>Must be between <see langword="1"/> and <see langword="100"/>. Defaults to <see langword="10"/>.</para>
        /// </summary>
        [Range(1, 100, ErrorMessage = "Size must be between 1 and 100")]
        public int Size { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether the results should be sorted in descending order.
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public bool IsDescending { get; set; } = false;

        /// <summary>
        /// Gets or sets the field by which the results should be sorted. Defaults to <c>id</c>.
        /// </summary>
        public string SortBy { get; set; } = "id";
    }
}

using System.ComponentModel.DataAnnotations;

namespace api.QueryObjects
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
        /// Gets the identifier of the user whose entities should be queried.
        /// </summary>
        /// <value>
        /// The user identifier, or <see langword="null"/>
        /// when the current authenticated user should be used.
        /// </value>
        public string? UserId { get; init; }
    }
}

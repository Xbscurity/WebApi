namespace api.Queries
{
    /// <summary>
    /// Represents query parameters for retrieving paginated entity collections for admin.
    /// </summary>
    /// <remarks>
    /// Extends <see cref="PagedQuery"/> with sorting and filtering options.
    /// </remarks>
    public record AdminEntityQuery : EntityQuery
    {
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

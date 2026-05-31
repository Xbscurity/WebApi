namespace api.Providers.CurrentUser
{
    /// <summary>
    /// Provides information about the current authenticated user.
    /// </summary>
    /// <remarks>
    /// This abstraction exposes commonly used user-related data
    /// from the current request context.
    /// </remarks>
    public interface ICurrentUser
    {
        /// <summary>
        /// Gets the identifier of the current authenticated user.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when accessed outside of a valid authenticated request context.
        /// </exception>
        public string UserId { get; }

        /// <summary>
        /// Gets a value indicating whether the current user
        /// belongs to the administrator role.
        /// </summary>
        public bool IsAdmin { get; }
    }
}

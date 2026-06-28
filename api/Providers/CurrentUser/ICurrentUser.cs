namespace api.Providers.CurrentUser
{
    /// <summary>
    /// Provides information about the current authenticated user.
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// Gets the identifier of the current authenticated user.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when accessed outside of a valid authenticated request context.
        /// </exception>
        string UserId { get; }
    }
}

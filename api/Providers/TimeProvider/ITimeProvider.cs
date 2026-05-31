namespace api.Providers.Time
{
    /// <summary>
    /// Provides access to the current UTC date and time.
    /// </summary>
    /// <remarks>
    /// This abstraction enables testable and centralized time retrieval
    /// throughout the application.
    /// </remarks>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets the current UTC date and time.
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }
}

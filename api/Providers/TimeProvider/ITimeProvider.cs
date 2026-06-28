namespace api.Providers.Time
{
    /// <summary>
    /// Provides access to the current UTC date and time.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets the current UTC date and time.
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }
}

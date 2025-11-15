namespace api.Providers.Interfaces
{
    /// <summary>
    /// Defines a contract for providing the current UTC time.
    /// </summary>
    /// <remarks>
    /// This abstraction allows time retrieval to be decoupled from the system clock,
    /// making it easier to test time-dependent logic by substituting with mock or
    /// custom implementations.
    /// </remarks>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets the current date and time in Coordinated Universal Time (UTC).
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }
}

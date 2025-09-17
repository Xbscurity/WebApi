using api.Providers.Interfaces;

namespace api.Providers
{
    /// <summary>
    /// Default implementation of <see cref="ITimeProvider"/> that uses the system clock.
    /// </summary>
    /// <remarks>
    /// This implementation retrieves the current time using <see cref="DateTimeOffset.UtcNow"/>.
    /// </remarks>
    public class UtcTimeProvider : ITimeProvider
    {
        /// <inheritdoc />
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}

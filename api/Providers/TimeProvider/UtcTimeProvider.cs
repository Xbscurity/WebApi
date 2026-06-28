using api.Providers.Time;

namespace api.Providers.TimeProvider
{
    /// <summary>
    /// Default implementation of <see cref="ITimeProvider"/>.
    /// </summary>
    public class UtcTimeProvider : ITimeProvider
    {
        /// <inheritdoc />
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}

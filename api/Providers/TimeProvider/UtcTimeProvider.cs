using api.Providers.Time;

namespace api.Providers.TimeProvider
{
    /// <summary>
    /// Default implementation of <see cref="ITimeProvider"/>.
    /// </summary>
    /// <remarks>
    /// This implementation uses <see cref="DateTimeOffset.UtcNow"/>
    /// as the source of time information.
    /// </remarks>
    public class UtcTimeProvider : ITimeProvider
    {
        /// <inheritdoc />
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}

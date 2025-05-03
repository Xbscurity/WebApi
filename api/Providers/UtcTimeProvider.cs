using api.Providers.Interfaces;

namespace api.Providers
{
    public class UtcTimeProvider : ITimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}

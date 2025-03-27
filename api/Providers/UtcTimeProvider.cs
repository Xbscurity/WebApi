using api.Services.Interfaces;

namespace api.Services
{
    public class UtcTimeProvider : ITimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}

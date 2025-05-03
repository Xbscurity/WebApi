namespace api.Providers.Interfaces
{
    public interface ITimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}

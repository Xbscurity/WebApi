namespace api.Services.Interfaces
{
    public interface ITimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}

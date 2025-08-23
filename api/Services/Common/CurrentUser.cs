namespace api.Services.Common
{
    public record CurrentUser
    {
        public string? UserId { get; init; }

        public bool IsAdmin { get; init; }

        public List<string> Roles { get; init; } = new();

    }
}

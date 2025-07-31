namespace api.Services.Token
{
    public class RefreshTokenResult
    {
        public bool IsSuccess => Error == null;

        public string? Error { get; set; }

        public string? NewAccessToken { get; set; }

        public string? NewRefreshToken { get; set; }

        public DateTimeOffset? ExpiresAt { get; set; }
    }
}

using api.Models;

namespace api.Services.Token
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(AppUser appUser);

        public string GenerateRefreshToken();

        public RefreshToken GenerateRefreshTokenEntity(string plainToken, AppUser user, string? ipAddress);

        public Task SaveRefreshTokenAsync(RefreshToken token);

        public Task<RefreshTokenResult> TryRefreshTokensAsync(string? refreshTokenPlain, string? ipAddress);

        Task RevokeRefreshTokenAsync(string token, string? ipAddress);
    }
}

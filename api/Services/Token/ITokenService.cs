using api.Models;

namespace api.Services.Token
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(AppUser appUser);

        string GenerateRefreshToken();

        RefreshToken GenerateRefreshTokenEntity(string plainToken, AppUser user, string? ipAddress);

        Task SaveRefreshTokenAsync(RefreshToken token);

        Task<RefreshTokenResult> TryRefreshTokensAsync(string? refreshTokenPlain, string? ipAddress);

        Task RevokeRefreshTokenAsync(string token, string? ipAddress, string reason);

        Task RevokeAllRefreshTokensAsync(string userId, string? ipAddress, string reason);
    }
}

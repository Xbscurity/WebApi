using api.Models;

namespace api.Services.Token
{
    /// <summary>
    /// Defines operations for generating, validating, refreshing, and revoking access and refresh tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a new JWT access token for the specified user.
        /// </summary>
        /// <param name="appUser">The application user for whom the token is generated.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the signed JWT access token.</returns>
        Task<string> GenerateAccessTokenAsync(AppUser appUser);

        /// <summary>
        /// Generates a new refresh token as a random string.
        /// </summary>
        /// <returns>A cryptographically secure random refresh token string.</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Creates a refresh token entity for persistence, including its hash and metadata.
        /// </summary>
        /// <param name="plainToken">The raw refresh token string.</param>
        /// <param name="user">The user to whom the token belongs.</param>
        /// <param name="ipAddress">The IP address from which the token was created, or <see langword="null"/> if unknown.</param>
        /// <returns>A <see cref="RefreshToken"/> entity ready for storage.</returns>
        RefreshToken GenerateRefreshTokenEntity(string plainToken, AppUser user, string? ipAddress);

        /// <summary>
        /// Persists a refresh token in the database.
        /// </summary>
        /// <param name="token">The refresh token entity to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SaveRefreshTokenAsync(RefreshToken token);

        /// <summary>
        /// Attempts to refresh access and refresh tokens using an existing refresh token.
        /// </summary>
        /// <param name="refreshTokenPlain">The plain refresh token string provided by the client.</param>
        /// <param name="ipAddress">The IP address of the request, or <see langword="null"/> if unavailable.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="RefreshTokenResult"/> with the outcome of the operation.
        /// </returns>
        Task<RefreshTokenResult> TryRefreshTokensAsync(string? refreshTokenPlain, string? ipAddress);

        /// <summary>
        /// Revokes a specific refresh token, preventing its future use.
        /// </summary>
        /// <param name="token">The plain refresh token string to revoke.</param>
        /// <param name="ipAddress">The IP address of the request, or <see langword="null"/>.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeRefreshTokenAsync(string token, string? ipAddress, string reason);

        /// <summary>
        /// Revokes all active refresh tokens for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose tokens should be revoked.</param>
        /// <param name="ipAddress">The IP address of the request, or <see langword="null"/>.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeAllRefreshTokensAsync(string userId, string? ipAddress, string reason);
    }
}

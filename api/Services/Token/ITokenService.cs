using api.Models;
using ErrorOr;

namespace api.Services.Token
{
    /// <summary>
    /// Provides functionality for generating and managing access and refresh tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token for the specified user.
        /// </summary>
        /// <param name="appUser">The user for whom the token is created.</param>
        /// <returns>The generated access token.</returns>
        Task<string> GenerateAccessTokenAsync(AppUser appUser);

        /// <summary>
        /// Validates a refresh token against stored data.
        /// </summary>
        /// <param name="refreshTokenPlain">The plain-text refresh token.</param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="RefreshToken"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<RefreshToken>> ValidateStoredTokenAsync(string? refreshTokenPlain);

        /// <summary>
        /// Replaces an existing refresh token and issues a new access token.
        /// </summary>
        /// <param name="user">The user associated with the token.</param>
        /// <param name="stored">The existing stored refresh token.</param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="RefreshTokenDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<RefreshTokenDto>> RotateTokensAsync(AppUser user, RefreshToken stored);

        /// <summary>
        /// Creates and stores a new refresh token for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>The plain-text refresh token.</returns>
        Task<string> CreateRefreshTokenAsync(string userId);

        /// <summary>
        /// Revokes a refresh token.
        /// </summary>
        /// <param name="token">The plain-text refresh token.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeRefreshTokenAsync(string token, string reason);
    }
}

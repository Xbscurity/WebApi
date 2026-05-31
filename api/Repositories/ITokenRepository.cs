using api.Models;

namespace api.Repositories
{
    /// <summary>
    /// Defines persistence operations for refresh tokens.
    /// </summary>
    public interface ITokenRepository
    {
        /// <summary>
        /// Retrieves a refresh token by its hashed value.
        /// </summary>
        /// <param name="hash">
        /// The hashed refresh token.
        /// </param>
        /// <returns>
        /// The matching <see cref="RefreshToken"/> if found; otherwise <see langword="null"/>.
        /// </returns>
        Task<RefreshToken?> GetByHashAsync(string hash);

        /// <summary>
        /// Adds a new refresh token to the persistence store.
        /// </summary>
        /// <param name="token">
        /// The refresh token to add.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddAsync(RefreshToken token);

        /// <summary>
        /// Updates an existing refresh token.
        /// </summary>
        /// <param name="token">
        /// The refresh token to update.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateAsync(RefreshToken token);

        /// <summary>
        /// Saves all pending changes made within the repository to the persistence store.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous save operation.
        /// </returns>
        Task SaveChangesAsync();

        /// <summary>
        /// Revokes a refresh token by its hashed value.
        /// </summary>
        /// <param name="tokenHash">
        /// The hashed value of the refresh token.
        /// </param>
        /// <param name="ipAddress">
        /// The IP address from which the revocation was performed, if available.
        /// </param>
        /// <param name="reason">
        /// The reason for revocation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeByHashAsync(string tokenHash, string? ipAddress, string reason);

        /// <summary>
        /// Revokes all active refresh tokens for the specified user.
        /// </summary>
        /// <param name="ipAddress">
        /// The IP address from which the revocation was performed, if available.
        /// </param>
        /// <param name="reason">
        /// The reason for revocation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeAllRefreshTokensAsync(string? ipAddress, string reason);
    }
}

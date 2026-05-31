using api.Dtos.Account;
using api.Dtos.User;
using api.Services.Token;
using ErrorOr;

namespace api.Services.Auth
{
    /// <summary>
    /// Defines authentication and session management operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="dto">
        /// The registration data required to create the user account.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="AuthResult"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<AuthResult>> RegisterAsync(RegisterInputDto dto);

        /// <summary>
        /// Authenticates a user using the provided credentials.
        /// </summary>
        /// <param name="dto">
        /// The login credentials.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="AuthResult"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<AuthResult>> LoginAsync(LoginInputDto dto);

        /// <summary>
        /// Refreshes the current user session using a refresh token.
        /// </summary>
        /// <param name="refreshToken">
        /// The refresh token associated with the session.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="RefreshTokenDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<RefreshTokenDto>> RefreshSessionAsync(string? refreshToken);
    }
}

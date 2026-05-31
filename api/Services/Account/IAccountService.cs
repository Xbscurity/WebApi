using api.Dtos.Account;
using api.Dtos.User;
using ErrorOr;

namespace api.Services.Account
{
    /// <summary>
    /// Defines operations related to the current authenticated user account.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Retrieves the profile information of the current authenticated user.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="UserProfileOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<UserProfileOutputDto>> GetProfileAsync();

        /// <summary>
        /// Changes the password of the current authenticated user.
        /// </summary>
        /// <param name="dto">
        /// The password change request containing the current
        /// and new passwords.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="string"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<string>> ChangePasswordAsync(ChangePasswordInputDto dto);
    }
}

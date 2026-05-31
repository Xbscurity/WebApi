using api.Dtos.User;
using api.QueryObjects;
using api.Services.Shared;
using ErrorOr;

namespace api.Services.UserManagement
{
    /// <summary>
    /// Provides operations for managing users.
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// Retrieves a paginated list of users.
        /// </summary>
        /// <param name="query">Query parameters including pagination, sorting, and filtering options.</param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="UserManagementUserOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<PagedItems<UserManagementUserOutputDto>>> GetAllUsersAsync(UserManagementQuery query);

        /// <summary>
        /// Retrieves a user by identifier.
        /// </summary>
        /// <param name="id">
        /// The identifier of the user to retrieve.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing the
        /// <see cref="UserManagementUserOutputDto"/> if successful;
        /// otherwise, an error.
        /// </returns>
        Task<ErrorOr<UserManagementUserOutputDto>> GetByIdAsync(string id);

        /// <summary>
        /// Updates the ban status of a user.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user whose ban status should be updated.
        /// </param>
        /// <param name="inputDto">
        /// The request containing the desired ban status.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing the updated
        /// <see cref="BanStatusOutputDto"/> if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<BanStatusOutputDto>> SetBanAsync(string userId, BanStatusInputDto inputDto);
    }
}
using api.Dtos.User;
using api.Models;
using Ardalis.Specification;
using Microsoft.AspNetCore.Identity;

namespace api.Services.User
{
    /// <summary>
    /// Provides the APIs for managing user in a persistence store.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets a list of users based on the provided specification.
        /// </summary>
        /// <param name="specification">The specification used to filter, sort, and project users.</param>
        /// <returns>A list of <see cref="UserManagementUserOutputDto"/> matching the specification.</returns>
        Task<List<UserManagementUserOutputDto>> GetAllAsync(ISpecification<AppUser, UserManagementUserOutputDto> specification);

        /// <summary>
        /// Finds and returns a user, if any, who has the specified <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The user ID to search for.</param>
        /// <returns>
        /// The user matching the specified <paramref name="userId"/> if it exists.
        /// </returns>
        Task<AppUser?> FindByIdAsync(string userId);

        /// <summary>
        /// Finds and returns a user, if any, who has the specified user name.
        /// </summary>
        /// <param name="userName">The user name to search for.</param>
        /// <returns>
        /// The user matching the specified <paramref name="userName"/> if it exists.
        /// </returns>
        Task<AppUser?> FindByNameAsync(string userName);

        /// <summary>
        /// Checks whether a user with the specified ID exists in the system.
        /// </summary>
        /// <param name="id">The user ID to check.</param>
        /// <returns><see langword="true"/> if a user with the ID exists; otherwise <see langword="false"/>.</returns>
        Task<bool> AnyAsync(string id);

        /// <summary>
        /// Returns the total number of users in the system.
        /// </summary>
        /// <returns>The total count of users.</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Gets a list of role names the specified <paramref name="user"/> belongs to.
        /// </summary>
        /// <param name="user">The user whose role names to retrieve.</param>
        /// <returns>A list of role names.</returns>
        Task<IList<string>> GetRolesAsync(AppUser user);

        /// <summary>
        /// Checks whether the currently logged-in user is banned.
        /// </summary>
        /// <returns><see langword="true"/> if the current user is banned; otherwise <see langword="false"/>.</returns>
        ValueTask<bool> IsBannedAsync();

        /// <summary>
        /// Creates the specified <paramref name="user"/> in the backing store with given password.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <param name="password">The password for the user to hash and store.</param>
        /// <returns>
        /// The <see cref="IdentityResult"/> of the operation.
        /// </returns>
        Task<IdentityResult> CreateAsync(AppUser user, string password);

        /// <summary>
        /// Updates the specified <paramref name="user"/> in the backing store.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>
        /// The <see cref="IdentityResult"/> of the operation.
        /// </returns>
        Task<IdentityResult> UpdateAsync(AppUser user);

        /// <summary>
        /// Add the specified <paramref name="user"/> to the named role.
        /// </summary>
        /// <param name="user">The user to add to the named role.</param>
        /// <param name="role">The name of the role to add the user to.</param>
        /// <returns>
        /// The <see cref="IdentityResult"/> of the operation.
        /// </returns>
        Task<IdentityResult> AddToRoleAsync(AppUser user, string role);

        /// <summary>
        /// Returns a flag indicating whether the given <paramref name="password"/> is valid for the
        /// specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user whose password should be validated.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <paramref name="password" /> matches the one store for the <paramref name="user"/>,
        /// otherwise <see langword="false"/>.</returns>
        Task<bool> CheckPasswordAsync(AppUser user, string password);

        /// <summary>
        /// Changes a user's password after confirming the specified <paramref name="currentPassword"/> is correct.
        /// </summary>
        /// <param name="user">The user whose password should be set.</param>
        /// <param name="currentPassword">The current password to validate before changing.</param>
        /// <param name="newPassword">The new password to set for the specified <paramref name="user"/>.</param>
        /// <returns>
        /// The <see cref="IdentityResult"/> of the operation.
        /// </returns>
        Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword);
    }
}
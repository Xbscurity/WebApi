using api.Controllers.User;

namespace api.Dtos.User
{
    /// <summary>
    /// Defines the output DTO containing user details used by <see cref="AdminUserManagementController"/>.
    /// </summary>
    public class AdminUserManagementUserOutputDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// Gets or sets the username chosen by the user.
        /// </summary>
        public string UserName { get; set; } = default!;

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// Gets or sets a value indicating whether the user is banned.
        /// <see langword="true"/> if the user is banned; otherwise, <see langword="false"/>.
        /// </summary>
        public bool IsBanned { get; set; } = default!;
    }
}

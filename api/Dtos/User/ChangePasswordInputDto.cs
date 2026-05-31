using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Account
{
    /// <summary>
    /// Represents the data required to change a user's password.
    /// </summary>
    public record ChangePasswordInputDto
    {
        /// <summary>
        /// Gets the user's current password.
        /// </summary>
        [Required]
        required public string CurrentPassword { get; init; }

        /// <summary>
        /// Gets the new password to assign to the account.
        /// </summary>
        [Required]
        required public string NewPassword { get; init; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Account
{
    /// <summary>
    /// Defines the input DTO for changing a user's password.
    /// </summary>
    public record ChangePasswordInputDto
    {
        /// <summary>
        /// Gets the current password of the user.
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        required public string CurrentPassword { get; init; }

        /// <summary>
        /// Gets the new password to be set for the user.
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        required public string NewPassword { get; init; }
    }
}

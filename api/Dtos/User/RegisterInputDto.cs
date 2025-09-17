using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Account
{
    /// <summary>
    /// Defines the input DTO for registering a new user.
    /// </summary>
    public record RegisterInputDto
    {
        /// <summary>
        /// Gets the desired username for the new user.
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        required public string UserName { get; init; }

        /// <summary>
        /// Gets the email address of the new user.
        /// <para>Must be a valid email format.</para>
        /// This property is required.
        /// </summary>
        [Required]
        [EmailAddress]
        required public string Email { get; init; }

        /// <summary>
        /// Gets the password for the new user account.
        /// <para>This field is required.</para>
        /// </summary>
        [Required]
        required public string Password { get; init; }
    }
}

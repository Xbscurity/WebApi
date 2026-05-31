using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Account
{
    /// <summary>
    /// Represents the data required to register a new user account.
    /// </summary>
    public record RegisterInputDto
    {
        /// <summary>
        /// Gets the username for the new account.
        /// </summary>
        [Required]
        required public string UserName { get; init; }

        /// <summary>
        /// Gets the email address for the new account.
        /// </summary>
        [Required]
        [EmailAddress]
        required public string Email { get; init; }

        /// <summary>
        /// Gets the password for the new account.
        /// </summary>
        [Required]
        required public string Password { get; init; }
    }
}

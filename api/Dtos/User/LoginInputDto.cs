using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Account
{
    /// <summary>
    /// Represents the credentials required for user authentication.
    /// </summary>
    public record LoginInputDto
    {
        /// <summary>
        ///  Gets the username of the account to authenticate.
        /// </summary>
        [Required]
        required public string UserName { get; init; }

        /// <summary>
        /// Gets the password associated with the account.
        /// </summary>
        [Required]
        required public string Password { get; init; }
    }
}

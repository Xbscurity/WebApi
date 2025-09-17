using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Account
{
    /// <summary>
    /// Defines the input DTO for a user login request.
    /// </summary>
    public record LoginInputDto
    {
        /// <summary>
        /// Gets the username of the user attempting to log in.
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        required public string UserName { get; init; }

        /// <summary>
        /// Gets the password of the user attempting to log in.
        /// <para>This property is required.</para>
        /// </summary>
        [Required]
        required public string Password { get; init; }
    }
}

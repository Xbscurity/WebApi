using System.ComponentModel.DataAnnotations;

namespace api.Dtos.User
{
    /// <summary>
    /// Represents the data required to update a user's ban status.
    /// </summary>
    public record BanStatusInputDto
    {
        /// <summary>
        /// Gets a value indicating whether the user account should be banned.
        /// </summary>
        [Required]
        required public bool IsBanned { get; init; }
    }
}
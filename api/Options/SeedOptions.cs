using System.ComponentModel.DataAnnotations;

namespace api.Options
{
    /// <summary>
    /// Represents configuration settings used for admin data seeding.
    /// </summary>
    public record SeedOptions
    {
        /// <summary>
        /// Gets the configuration section name for seed settings.
        /// </summary>
        public const string SectionName = "Seed";

        /// <summary>
        /// Gets the email address of the initial administrator account.
        /// </summary>
        [Required]
        [EmailAddress]
        required public string AdminEmail { get; init; }

        /// <summary>
        /// Gets the password of the initial administrator account.
        /// </summary>
        [Required]
        required public string AdminPassword { get; init; }
    }
}

using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    /// <summary>
    /// Represents an application user entity extending ASP.NET Core Identity.
    /// </summary>
    public class AppUser : IdentityUser, ITrackedEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user is banned.
        /// </summary>
        public bool IsBanned { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset CreatedAt { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
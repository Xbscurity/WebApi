using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    /// <summary>
    /// Represents an application user in the system, extending <see cref="IdentityUser"/>.
    /// </summary>
    public class AppUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user is banned.
        /// </summary>
        public bool IsBanned { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the user was created.
        /// Defaults to the current UTC time.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}

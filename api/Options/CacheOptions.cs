using System.ComponentModel.DataAnnotations;

namespace api.Options
{
    /// <summary>
    /// Represents configuration settings for application caching.
    /// </summary>
    /// <remarks>
    /// These settings define cache-related expiration policies
    /// used throughout the application.
    /// </remarks>
    public record CacheOptions
    {
        /// <summary>
        /// Gets the configuration section name for cache settings.
        /// </summary>
        public const string SectionName = "Cache";

        /// <summary>
        /// Gets the cache duration used for banned user entries.
        /// </summary>
        /// <value>
        /// TTL for cached banned user data.
        /// </value>
        [Required]
        public TimeSpan BanUserTtl { get; init; }
    }
}

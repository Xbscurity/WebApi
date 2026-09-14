using System.ComponentModel.DataAnnotations;

namespace api.Options
{
    /// <summary>
    /// Represents configuration settings for a fixed-window rate limiter.
    /// </summary>
    public record RateLimitOptions
    {
        /// <summary>
        /// Gets the maximum number of requests allowed within the window.
        /// </summary>
        [Range(1, 1_000_000)]
        public int PermitLimit { get; init; } = 100;

        /// <summary>
        /// Gets the length of the fixed window, in seconds.
        /// </summary>
        [Range(1, 3600)]
        public int WindowSeconds { get; init; } = 10;
    }

    /// <summary>
    /// Represents configuration settings for rate limiting policies across the API.
    /// </summary>
    public record RateLimitingOptions
    {
        /// <summary>
        /// Gets the configuration section name for rate limiting settings.
        /// </summary>
        public const string SectionName = "RateLimiting";

        /// <summary>
        /// Gets the settings for the global rate limiter applied to all endpoints.
        /// </summary>
        [Required]
        public RateLimitOptions Global { get; init; } = new() { PermitLimit = 100, WindowSeconds = 10 };

        /// <summary>
        /// Gets the settings for the "auth" policy applied to authentication endpoints.
        /// </summary>
        [Required]
        public RateLimitOptions Auth { get; init; } = new() { PermitLimit = 5, WindowSeconds = 60 };
    }
}
using System.ComponentModel.DataAnnotations;

namespace api.Options
{
    /// <summary>
    /// Represents configuration settings for refresh token generation and expiration.
    /// </summary>
    public record RefreshTokenOptions
    {
        /// <summary>
        /// Gets the configuration section name for refresh token settings.
        /// </summary>
        public const string SectionName = "RefreshToken";

        /// <summary>
        /// Gets the number of days a refresh token remains valid.
        /// </summary>
        /// <value>
        /// The refresh token expiration period in days.
        /// Defaults to <c>14</c>.
        /// </value>
        [Range(1, 30)]
        public int ExpirationDays { get; init; } = 14;

        /// <summary>
        /// Gets the generated refresh token length.
        /// </summary>
        [Range(16, 128)]
        public int Length { get; init; } = 64;
    }
}

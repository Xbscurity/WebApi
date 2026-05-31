using System.ComponentModel.DataAnnotations;

namespace api.Options
{
    /// <summary>
    /// Represents configuration settings for JWT authentication.
    /// </summary>
    /// <remarks>
    /// These settings are used for generating and validating JWT access tokens.
    /// </remarks>
    public record JwtOptions
    {
        /// <summary>
        /// Gets the configuration section name for JWT settings.
        /// </summary>
        public const string SectionName = "JWT";

        /// <summary>
        /// Gets the token issuer.
        /// </summary>
        /// <value>
        /// The issuer identifier used when generating JWT tokens.
        /// </value>
        [Required]
        required public string Issuer { get; init; }

        /// <summary>
        /// Gets the intended token audience.
        /// </summary>
        /// <value>
        /// The audience identifier used when validating JWT tokens.
        /// </value>
        [Required]
        required public string Audience { get; init; }

        /// <summary>
        /// Gets the secret signing key used to sign JWT tokens.
        /// </summary>
        /// <value>
        /// The symmetric signing key.
        /// </value>
        [Required]
        required public string SigningKey { get; init; }

        /// <summary>
        /// Gets the access token expiration time in minutes.
        /// </summary>
        /// <value>
        /// The token lifetime in minutes.
        /// Defaults to <c>15</c>.
        /// </value>
        [Range(1, 1440)]
        public double ExpirationMinutes { get; init; } = 15;
    }
}

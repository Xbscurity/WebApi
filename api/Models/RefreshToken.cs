namespace api.Models
{
    /// <summary>
    /// Defines a refresh token entity stored in the database.
    /// </summary>
    /// <remarks>
    /// This model is used to persist information about user refresh tokens,
    /// including token hash, creation/expiration times, IP addresses, and revocation details.
    /// </remarks>
    public class RefreshToken
    {
        /// <summary>
        /// Gets or sets primary key identifier of the refresh token record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets cryptographic hash of the refresh token value.
        /// </summary>
        public string TokenHash { get; set; }

        /// <summary>
        /// Gets or sets identifier of the user associated with this token.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets uTC timestamp when the token was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets iP address from which the token was created.
        /// </summary>
        public string CreatedByIp { get; set; }

        /// <summary>
        /// Gets or sets uTC timestamp when the token will expire.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// UTC timestamp when the token was revoked, if applicable.
        /// </summary>
        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>
        /// Gets or sets iP address from which the token was revoked, if applicable.
        /// </summary>
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// Gets or sets hash (or identifier) of the new token that replaced this one.
        /// </summary>
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Gets or sets reason for token revocation (e.g., logout, compromise).
        /// </summary>
        public string? Reason { get; set; }
    }
}

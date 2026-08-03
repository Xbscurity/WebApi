namespace api.Models
{
    /// <summary>
    /// Represents a refresh token used for authentication session management.
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// Gets or sets the hashed value of the refresh token.
        /// </summary>
        required public string TokenHash { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who owns this token.
        /// </summary>
        required public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the IP address from which the token was created.
        /// </summary>
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// Gets or sets the expiration time of the refresh token.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the token was revoked.
        /// </summary>
        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>
        /// Gets a value indicating whether the token has been revoked.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if <see cref="RevokedAt"/> has a value;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool IsRevoked => RevokedAt != null;

        /// <summary>
        /// Gets or sets the IP address from which the token was revoked.
        /// </summary>
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// Gets or sets the token that replaced this refresh token, if any.
        /// </summary>
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Gets or sets the reason for token revocation.
        /// </summary>
        public string? Reason { get; set; }
    }
}

namespace api.Services.Token
{
    /// <summary>
    /// Represents the result of a refresh token operation.
    /// </summary>
    public class RefreshTokenResult
    {
        /// <summary>
        /// Gets a value indicating whether the refresh operation succeeded.
        /// </summary>
        public bool IsSuccess => Error == null;

        /// <summary>
        /// Gets or sets an error message if the refresh operation failed; otherwise, <see langword="null"/>.
        /// </summary>.
        public string? Error { get; set; }

        /// <summary>
        /// Gets or sets the newly issued access token, if the operation succeeded.
        /// </summary>
        public string? NewAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the newly issued refresh token, if the operation succeeded.
        /// </summary>
        public string? NewRefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time of the new refresh token, if issued.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }
    }
}

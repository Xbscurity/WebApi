namespace api.Services.Token
{
    /// <summary>
    /// Represents a pair of authentication tokens issued after a successful rotation operation.
    /// </summary>
    public record RefreshTokenDto
    {
        /// <summary>
        /// Gets the newly generated JWT access token.
        /// </summary>
        required public string AccessToken { get; init; }

        /// <summary>
        /// Gets the new cryptographically strong refresh token.
        /// </summary>
        required public string RefreshToken { get; init; }
    }
}

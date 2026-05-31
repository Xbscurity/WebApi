namespace api.Dtos.Account
{
    /// <summary>
    /// Represents the response returned after a successful token refresh operation.
    /// </summary>
    public record RefreshTokenOutputDto
    {
        /// <summary>
        /// Gets the newly generated JWT access token.
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        required public string AccessToken { get; init; }
    }
}

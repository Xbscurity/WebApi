namespace api.Dtos.User
{
    /// <summary>
    /// Represents the authentication result  used in the application layer.
    /// </summary>
    public record AuthResult
    {
        /// <summary>
        /// Gets the username of the authenticated user.
        /// </summary>
        required public string UserName { get; init; }

        /// <summary>
        /// Gets the email address of the authenticated user.
        /// </summary>
        required public string Email { get; init; }

        /// <summary>
        /// Gets the JWT access token used for authenticated API requests.
        /// </summary>
        required public string AccessToken { get; init; }

        /// <summary>
        /// Gets the refresh token used to obtain new access tokens
        /// without re-authentication.
        /// </summary>
        required public string RefreshToken { get; init; }
    }
}

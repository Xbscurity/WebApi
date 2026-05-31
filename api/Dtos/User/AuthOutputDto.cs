namespace api.Dtos.User
{
    /// <summary>
    /// Represents the authentication response returned after successful login or registration.
    /// </summary>
    public record AuthOutputDto
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
        /// Gets the JWT access token used for API authentication.
        /// </summary>
        required public string AccessToken { get; init; }
    }
}

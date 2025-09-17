namespace api.Dtos.Account
{
    /// <summary>
    /// Defines the output DTO containing user details returned after registration or login.
    /// </summary>
    public record AccountUserOutputDto
    {
        /// <summary>
        /// Gets the username of the user.
        /// </summary>
        public string UserName { get; init; } = default!;

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        public string Email { get; init; } = default!;

        /// <summary>
        /// Gets the JWT access token issued for the user.
        /// </summary>
        public string Token { get; init; } = default!;

    }
}
namespace api.Dtos.User
{
    /// <summary>
    /// Represents profile information for an authenticated user.
    /// </summary>
    public record UserProfileOutputDto
    {
        /// <summary>
        /// Gets the username of the user.
        /// </summary>
        required public string UserName { get; init; }

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        required public string Email { get; init; }

        /// <summary>
        /// Gets the date and time when the user account was created.
        /// </summary>
        /// <example>2026-05-14T10:30:00Z.</example>
        required public DateTimeOffset CreatedAt { get; init; }
    }
}

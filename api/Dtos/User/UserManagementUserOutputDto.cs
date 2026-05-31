namespace api.Dtos.User
{
    /// <summary>
    /// Represents user information returned for user management operations.
    /// </summary>
    public record UserManagementUserOutputDto
    {
        /// <summary>
        /// Gets the unique identifier of the user.
        /// </summary>
        /// <example>f8c2f7d4-8f2b-4b89-9f6f-1d7a8f4e2c11.</example>
        required public string Id { get; init; }

        /// <summary>
        /// Gets the username of the user.
        /// </summary>
        public string? UserName { get; init; }

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// Gets a value indicating whether the user account is banned.
        /// </summary>
        required public bool IsBanned { get; init; }

        /// <summary>
        /// Gets the date and time when the user account was created.
        /// </summary>
        /// <example>2026-05-14T10:30:00Z.</example>
        required public DateTimeOffset CreatedAt { get; init; }
    }
}

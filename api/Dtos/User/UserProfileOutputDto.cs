namespace api.Dtos.Account
{
    /// <summary>
    /// Defines the output DTO containing a user's profile information.
    /// </summary>
    public class UserProfileOutputDto
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
        /// Gets the date and time when the user was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }
    }
}

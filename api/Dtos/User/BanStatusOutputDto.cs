namespace api.Dtos.User
{
    /// <summary>
    /// Represents the ban status of a user account.
    /// </summary>
    public record BanStatusOutputDto
    {
        /// <summary>
        /// Gets a value indicating whether the user account is banned.
        /// </summary>
        required public bool BanStatus { get; init; }
    }
}

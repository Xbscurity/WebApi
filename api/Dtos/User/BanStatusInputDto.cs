namespace api.Dtos.User
{
    /// <summary>
    /// Defines the input DTO for changing user's ban status.
    /// </summary>
    public class BanStatusInputDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user is banned.
        /// <see langword="true"/> if the user is banned; otherwise, <see langword="false"/>.
        /// </summary>
        public bool IsBanned { get; set; }
    }
}

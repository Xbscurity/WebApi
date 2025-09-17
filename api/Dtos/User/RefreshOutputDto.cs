namespace api.Dtos.Account
{
    /// <summary>
    /// Defines the output DTO returned when an access token is refreshed.
    /// </summary>
    public record RefreshOutputDto
    {
        /// <summary>
        /// Gets the newly issued access token.
        /// </summary>
        public string Token { get; init; } = default!;
    }
}

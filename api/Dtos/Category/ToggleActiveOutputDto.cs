namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the result of toggling an category's active status.
    /// </summary>
    public record ToggleActiveOutputDto
    {
        /// <summary>
        /// Gets a value indicating whether the current active status after the toggle operation.
        /// </summary>
        required public bool ToggleActive { get; init; }
    }
}
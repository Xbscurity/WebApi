namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the result of setting an category's active status.
    /// </summary>
    public record SetActiveOutputDto
    {
        /// <summary>
        /// Gets a value indicating whether the current active status after the toggle operation.
        /// </summary>
        required public bool IsActive { get; init; }
    }
}
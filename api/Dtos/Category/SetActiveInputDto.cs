namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the request body for toggling a category's active status.
    /// </summary>
    public class SetActiveInputDto
    {
        /// <summary>
        /// Gets a value indicating whether the current active status after the toggle operation.
        /// </summary>
        required public bool IsActive { get; init; }
    }
}

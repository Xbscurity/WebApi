namespace api.Dtos.Category
{
    /// <summary>
    /// Represents a category returned by the application.
    /// </summary>
    public record CategoryOutputDto
    {
        /// <summary>
        /// Gets the unique identifier of the category.
        /// </summary>
        required public Guid Id { get; init; }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        required public string Name { get; init; }

        /// <summary>
        /// Gets the identifier of the user who owns the category.
        /// </summary>
        public string? AppUserId { get; init; }

        /// <summary>
        /// Gets a value indicating whether the category is active and available for use.
        /// </summary>
        required public bool IsActive { get; init; }

        /// <summary>
        /// Gets the date and time when the transaction was created.
        /// </summary>
        required public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Gets the date and time when the transaction was updated.
        /// </summary>
        required public DateTimeOffset UpdatedAt { get; init; }
    }
}

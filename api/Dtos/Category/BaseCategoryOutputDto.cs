namespace api.Dtos.Category
{
    /// <summary>
    /// Represents the output DTO containing the basic information of a category.
    /// </summary>
    public record BaseCategoryOutputDto
    {
        /// <summary>
        /// Gets the unique identifier of the category.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string Name { get; init; } = default!;

        /// <summary>
        /// Gets the ID of the user who owns this category.
        /// <see langword="Null"/> indicates a global/common category.
        /// </summary>
        public string? AppUserId { get; init; }

        /// <summary>
        /// Gets a value indicating whether the category is active.
        /// Default is <see langword="true"/>.
        /// </summary>
        public bool IsActive { get; init; } = true;
    }
}

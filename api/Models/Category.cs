namespace api.Models
{
    /// <summary>
    /// Represents a category used to classify <see cref="FinancialTransaction"/> in the application.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Gets or sets the unique identifier of the category.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the user who owns this category, if applicable.
        /// </summary>
        public string? AppUserId { get; set; }

        /// <summary>
        /// Gets or sets the user who owns this category, if applicable.
        /// </summary>
        public AppUser? AppUser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the category is active.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

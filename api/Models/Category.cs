namespace api.Models
{
    /// <summary>
    /// Represents a financial transaction category owned by a user.
    /// </summary>
    /// <remarks>
    /// Categories are used to group financial transactions and support
    /// user-specific organization of financial data.
    /// </remarks>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        required public string Name { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who owns this category.
        /// </summary>
        required public string AppUserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the owning user.
        /// </summary>
        public AppUser AppUser { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the category is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

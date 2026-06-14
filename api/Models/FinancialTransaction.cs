using api.Enums;

namespace api.Models
{
    /// <summary>
    /// Represents a financial transaction recorded in the system.
    /// </summary>
    /// <remarks>
    /// Each transaction belongs to a category and is associated with a user.
    /// </remarks>
    public class FinancialTransaction : BaseEntity
    {
        /// <summary>
        /// Gets or sets the category identifier associated with the transaction.
        /// </summary>
        required public Guid CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the transaction category.
        /// </summary>
        public Category Category { get; set; } = null!;

        /// <summary>
        /// Gets or sets the type of the financial transaction.
        /// </summary>
        required public FinancialTransactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the monetary amount of the transaction.
        /// </summary>
        required public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets an optional comment describing the transaction.
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the user who owns this transaction.
        /// </summary>
        required public string AppUserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the owning user.
        /// </summary>
        public AppUser AppUser { get; set; } = null!;
    }
}
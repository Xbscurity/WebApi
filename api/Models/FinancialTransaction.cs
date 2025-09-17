using api.Providers.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    /// <summary>
    /// Defines a financial transaction made by a user in the application.
    /// </summary>
    public class FinancialTransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransaction"/> class
        /// using the specified time provider.
        /// </summary>
        /// <param name="timeProvider">The provider used to obtain the current UTC time.</param>
        public FinancialTransaction(ITimeProvider timeProvider)
        {
            CreatedAt = timeProvider.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransaction"/> class.
        /// Required by Entity Framework Core for object materialization. Must be parameterless.
        /// </summary>
        private FinancialTransaction()
        {
        }

        /// <summary>
        /// Gets or sets the unique identifier of the transaction.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the category associated with this transaction.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the category associated with this transaction.
        /// </summary>
        public Category Category { get; set; } = default!;

        /// <summary>
        /// Gets or sets the transaction amount.
        /// </summary>
        [Column(TypeName = "numeric(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets an optional comment or description for the transaction.
        /// Defaults to an empty string.
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the transaction was created.
        /// Initialized via <see cref="ITimeProvider"/> to ensure UTC consistency.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who made the transaction.
        /// </summary>
        public string AppUserId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the user who made the transaction.
        /// </summary>
        public AppUser AppUser { get; set; } = default!;

    }
}
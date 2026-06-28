using api.Enums;

namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents a financial transaction returned by the application as an administrator.
    /// </summary>
    public record AdminFinancialTransactionOutputDto
    {
        /// <summary>
        /// Gets the unique identifier of the financial transaction.
        /// </summary>
        required public Guid Id { get; init; }

        /// <summary>
        /// Gets the identifier of the category assigned to the transaction.
        /// </summary>
        required public Guid CategoryId { get; init; }

        /// <summary>
        /// Gets the transaction amount.
        /// </summary>
        required public decimal Amount { get; init; }

        /// <summary>
        /// Gets the type of the financial transaction.
        /// </summary>
        required public FinancialTransactionType Type { get; init; }

        /// <summary>
        /// Gets the comment or description associated with the transaction.
        /// </summary>
        required public string Comment { get; init; }

        /// <summary>
        /// Gets the date and time when the transaction was created.
        /// </summary>
        required public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Gets the date and time when the transaction was updated.
        /// </summary>
        required public DateTimeOffset UpdatedAt { get; init; }

        /// <summary>
        /// Gets the identifier of the user who owns the transaction.
        /// </summary>
        required public string AppUserId { get; init; }
    }
}

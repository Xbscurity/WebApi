namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents the output DTO for retrieving a minimal financial transaction details.
    /// </summary>
    public class BaseFinancialTransactionMinimalOutputDto
    {
        /// <summary>
        /// Gets the unique identifier of the transaction.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the unique identifier of the category.
        /// </summary>
        public int CategoryId { get; init; }

        /// <summary>
        /// Gets the transaction amount.
        /// Can be positive (income) or negative (expense).
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Gets the comment or description associated with the transaction.
        /// </summary>
        public string Comment { get; init; } = default!;

        /// <summary>
        /// Gets the date and time when the transaction was created (in UTC).
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Gets the identifier of the user who owns the transaction.
        /// </summary>
        public string AppUserId { get; init; } = default!;
    }
}

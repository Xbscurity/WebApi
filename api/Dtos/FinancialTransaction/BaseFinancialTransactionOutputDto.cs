namespace api.Dtos.FinancialTransaction
{
    public record BaseFinancialTransactionOutputDto
    {
        /// <summary>
        /// Gets the unique identifier of the transaction.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the name of the category associated with the transaction.
        /// Can be <see langword="null"/> if the category was deleted or is unavailable.
        /// </summary>
        public string? CategoryName { get; init; } = default!;

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

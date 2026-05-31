namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents grouped financial transaction report data.
    /// </summary>
    public record GroupedReportOutputDto
    {
        /// <summary>
        /// Gets the grouping key associated with the report result.
        /// </summary>
        required public ReportKey GroupKey { get; init; }

        /// <summary>
        /// Gets the total number of transactions in the group.
        /// </summary>
        required public int Count { get; init; }

        /// <summary>
        /// Gets the aggregated transaction amount for the group.
        /// </summary>
        required public decimal TotalAmount { get; init; }

        /// <summary>
        /// Gets the transactions included in the group.
        /// </summary>
        required public IReadOnlyList<FinancialTransactionOutputDto> Transactions { get; init; }
    }
}
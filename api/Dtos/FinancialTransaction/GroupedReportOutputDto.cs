namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents a grouped report output DTO containing a set of financial transactions.
    /// </summary>
    /// <remarks>
    /// Each group is identified by a <see cref="ReportKey"/> according to the selected strategy.
    /// </remarks>
    public record GroupedReportOutputDto
    {
        /// <summary>
        /// Gets the key that identifies this report group (e.g., by category or by date).
        /// </summary>
        required public ReportKey Key { get; init; }

        /// <summary>
        /// Gets the collection of transactions that belong to this group.
        /// </summary>
        public IReadOnlyList<BaseFinancialTransactionOutputDto> Transactions { get; init; }
            = new List<BaseFinancialTransactionOutputDto>();
    }
}
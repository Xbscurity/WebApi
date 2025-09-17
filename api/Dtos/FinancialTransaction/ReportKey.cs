namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents a grouping key used in financial transaction reports.
    /// </summary>
    /// <remarks>
    /// A report can be grouped by one of the following strategies:
    /// <list type="number">
    /// <item><description><b>By year and month</b> – groups transactions by their transaction date (year and month).</description></item>
    /// <item><description><b>By category</b> – groups transactions by their assigned category.</description></item>
    /// <item><description><b>By category, year, and month</b> – groups transactions simultaneously by category and transaction date.</description></item>
    /// </list>
    /// Depending on the selected strategy, some properties may be <see langword="null"/>.
    /// </remarks>
    public record ReportKey
    {
        /// <summary>
        /// Gets the category name used for grouping, if applicable.
        /// </summary>
        public string? Category { get; init; }

        /// <summary>
        /// Gets the year used for grouping, if applicable.
        /// </summary>
        public int? Year { get; init; }

        /// <summary>
        /// Gets the month used for grouping, if applicable.
        /// </summary>
        public int? Month { get; init; }
    }
}

using api.Services.FinancialTransactions;

namespace api.Queries
{
    /// <summary>
    /// Represents query parameters for generating grouped financial reports.
    /// </summary>
    /// <remarks>
    /// Extends <see cref="PagedQuery"/> with date filtering
    /// and grouping options.
    /// </remarks>
    public record ReportQuery : PagedQuery
    {
        /// <summary>
        /// Gets the inclusive start date used for report filtering.
        /// </summary>
        /// <value>
        /// The start date, or <see langword="null"/>
        /// when no lower date boundary is applied.
        /// </value>
        public DateTimeOffset? StartDate { get; init; }

        /// <summary>
        /// Gets the inclusive end date used for report filtering.
        /// </summary>
        /// <value>
        /// The end date, or <see langword="null"/>
        /// when no upper date boundary is applied.
        /// </value>
        public DateTimeOffset? EndDate { get; init; }

        /// <summary>
        /// Gets the grouping strategy used for report aggregation.
        /// </summary>
        /// <value>
        /// The grouping strategy key.
        /// Defaults to <see cref="GroupingReportStrategyKey.ByCategory"/>.
        /// </value>
        public GroupingReportStrategyKey Key { get; init; } = GroupingReportStrategyKey.ByCategory;

        /// <summary>
        /// Gets a value indicating whether inactive categories
        /// should be included in the report.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to include inactive categories;
        /// otherwise, <see langword="false"/>.
        /// Defaults to <see langword="false"/>.
        /// </value>
        public bool IncludeInactive { get; init; } = false;
    }
}
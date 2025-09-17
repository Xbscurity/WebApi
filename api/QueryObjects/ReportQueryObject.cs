using api.Enums;

namespace api.QueryObjects
{
    /// <summary>
    /// Defines query parameters for generating financial transaction reports.
    /// Inherits <see cref="PaginationQueryObject"/> and include filtering by date range
    /// and grouping strategy.
    /// </summary>
    public record ReportQueryObject : PaginationQueryObject
    {
        /// <summary>
        /// Gets or sets the start date for filtering transactions, if applicable.
        /// </summary>
        public DateTimeOffset? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for filtering transactions, if applicable.
        /// </summary>
        public DateTimeOffset? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the grouping strategy key that determines
        /// how transactions are aggregated in the report.
        /// <para>The default is <see cref="GroupingReportStrategyKey.ByCategory"/>.</para>
        /// </summary>
        public GroupingReportStrategyKey Key { get; set; } = GroupingReportStrategyKey.ByCategory;
    }
}
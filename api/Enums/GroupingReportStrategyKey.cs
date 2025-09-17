namespace api.Enums
{
    /// <summary>
    /// Defines available strategies for grouping financial transactions in reports.
    /// </summary>
    public enum GroupingReportStrategyKey
    {
        /// <summary>
        /// Groups transactions by category.
        /// </summary>
        ByCategory = 1,

        /// <summary>
        /// Groups transactions by year and month (date-based grouping).
        /// </summary>
        ByDate = 2,

        /// <summary>
        /// Groups transactions by both category and date (year and month).
        /// </summary>
        ByCategoryAndDate = 3,
    }
}

using api.Enums;

namespace api.QueryObjects
{
    public record ReportQueryObject : PaginationQueryObject
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public GroupingReportStrategyKey Key { get; set; } = GroupingReportStrategyKey.ByCategory;
    }
}
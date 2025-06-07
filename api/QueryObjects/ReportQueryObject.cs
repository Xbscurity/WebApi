using api.Enums;
using Microsoft.AspNetCore.Mvc;

namespace api.QueryObjects
{
    public record ReportQueryObject : PaginationQueryObject
    {
        [FromQuery(Name = "startDate")]
        public DateTimeOffset? StartDate { get; set; }

        [FromQuery(Name = "endDate")]
        public DateTimeOffset? EndDate { get; set; }

        public GroupingReportStrategyKey Key { get; set; } = GroupingReportStrategyKey.ByCategory;
    }
}
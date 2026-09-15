using api.Dtos.FinancialTransaction;
using api.Models;
using api.Queries;
using Ardalis.Specification;

namespace api.Services.FinancialTransactions
{
    /// <summary>
    /// Defines a strategy for grouping financial transaction reports.
    /// </summary>
    public interface IGroupingReportStrategy
    {
        /// <summary>
        /// Gets the unique key that identifies the grouping strategy.
        /// </summary>
        GroupingReportStrategyKey Key { get; }

        /// <summary>
        /// Retrieves grouped financial transaction report data.
        /// </summary>
        /// <param name="spec">
        /// The specification used to filter financial transactions.
        /// </param>
        /// <param name="query">
        /// The query containing grouping and pagination parameters.
        /// </param>
        /// <returns>
        /// The current page of grouped report items, together with the
        /// total number of groups across all pages.
        /// </returns>
        Task<(List<GroupedReportOutputDto> Items, int TotalCount)> GetGroupedAsync(Specification<FinancialTransaction> spec, ReportQuery query);
    }
}

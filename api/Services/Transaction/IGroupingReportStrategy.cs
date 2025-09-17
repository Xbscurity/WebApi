using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Models;

namespace api.Services.Transaction
{
    /// <summary>
    /// Defines a strategy for grouping financial transactions into reports.
    /// </summary>
    public interface IGroupingReportStrategy
    {
        /// <summary>
        /// Gets the key that identifies this grouping strategy.
        /// </summary>
        GroupingReportStrategyKey Key { get; }

        /// <summary>
        /// Groups transactions based on the strategy implementation.
        /// </summary>
        /// <param name="queryable">The queryable source of financial transactions.</param>
        /// <returns>A task that returns a grouped report.</returns>
        Task<List<GroupedReportOutputDto>> GroupAsync(IQueryable<FinancialTransaction> queryable);
    }
}

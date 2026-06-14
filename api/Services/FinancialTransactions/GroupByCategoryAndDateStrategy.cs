using api.Dtos.FinancialTransaction;
using api.Interfaces;
using api.Models;
using api.Queries;
using Ardalis.Specification;

namespace api.Services.FinancialTransactions
{
    /// <summary>
    /// Provides grouping logic for financial transaction reports by category and date.
    /// </summary>
    public class GroupByCategoryAndDateStrategy : IGroupingReportStrategy
    {
        private readonly IFinancialTransactionRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupByCategoryAndDateStrategy"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository used to retrieve grouped financial transaction data.
        /// </param>
        public GroupByCategoryAndDateStrategy(IFinancialTransactionRepository repository)
            => _repository = repository;

        /// <inheritdoc/>
        public GroupingReportStrategyKey Key => GroupingReportStrategyKey.ByCategoryAndDate;

        /// <inheritdoc/>
        public Task<List<GroupedReportOutputDto>> GetGroupedAsync(Specification<FinancialTransaction> spec, ReportQuery query)
            => _repository.GetGroupedListByCategoryAndDate(spec, query);
    }
}

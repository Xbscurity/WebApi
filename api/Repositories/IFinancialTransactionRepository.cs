using api.Dtos.FinancialTransaction;
using api.Models;
using api.QueryObjects;
using Ardalis.Specification;

namespace api.Interfaces
{
    /// <summary>
    /// Defines specialized repository operations for financial transaction reporting.
    /// </summary>
    /// <remarks>
    /// Provides grouped reporting queries for financial transactions
    /// using specification-based filtering.
    /// </remarks>
    public interface IFinancialTransactionRepository : IRepositoryBase<FinancialTransaction>
    {
        /// <summary>
        /// Retrieves grouped financial transaction data aggregated by category.
        /// </summary>
        /// <param name="spec">
        /// The specification used to filter financial transactions.
        /// </param>
        /// <param name="query">
        /// The report query containing paging parameters.
        /// </param>
        /// <returns>
        /// A collection of grouped report results aggregated by category.
        /// </returns>
        Task<List<GroupedReportOutputDto>> GetGroupedListByCategory(ISpecification<FinancialTransaction> spec, ReportQuery query);

        /// <summary>
        /// Retrieves grouped financial transaction data aggregated by date.
        /// </summary>
        /// <param name="spec">
        /// The specification used to filter financial transactions.
        /// </param>
        /// <param name="query">
        /// The report query containing paging parameters.
        /// </param>
        /// <returns>
        /// A collection of grouped report results aggregated by date.
        /// </returns>
        Task<List<GroupedReportOutputDto>> GetGroupedListByDate(ISpecification<FinancialTransaction> spec, ReportQuery query);

        /// <summary>
        /// Retrieves grouped financial transaction data aggregated
        /// by category and date.
        /// </summary>
        /// <param name="spec">
        /// The specification used to filter financial transactions.
        /// </param>
        /// <param name="query">
        /// The report query containing paging parameters.
        /// </param>
        /// <returns>
        /// A collection of grouped report results aggregated
        /// by category and date.
        /// </returns>
        Task<List<GroupedReportOutputDto>> GetGroupedListByCategoryAndDate(ISpecification<FinancialTransaction> spec, ReportQuery query);
    }
}

using api.Dtos.FinancialTransaction;
using api.Models;
using api.Queries;
using Ardalis.Specification;

namespace api.Repositories
{
    /// <summary>
    /// Defines  repository for managing financial transactions.
    /// </summary>
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
        /// The current page of grouped report results aggregated by category,
        /// together with the total number of groups across all pages.
        /// </returns>
        Task<(List<GroupedReportOutputDto> Items, int TotalCount)> GetGroupedListByCategory(
            ISpecification<FinancialTransaction> spec, ReportQuery query);

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
        /// The current page of grouped report results aggregated by date,
        /// together with the total number of groups across all pages.
        /// </returns>
        Task<(List<GroupedReportOutputDto> Items, int TotalCount)> GetGroupedListByDate(
            ISpecification<FinancialTransaction> spec, ReportQuery query);

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
        /// The current page of grouped report results aggregated
        /// by category and date, together with the total number
        /// of groups across all pages.
        /// </returns>
        Task<(List<GroupedReportOutputDto> Items, int TotalCount)> GetGroupedListByCategoryAndDate(
            ISpecification<FinancialTransaction> spec, ReportQuery query);
    }
}

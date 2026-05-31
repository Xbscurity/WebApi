using api.Dtos.FinancialTransaction;
using api.QueryObjects;
using api.Services.Shared;
using ErrorOr;

namespace api.Services.FinancialTransactions
{
    /// <summary>
    /// Defines operations for managing financial transactions and generating reports.
    /// </summary>
    public interface IFinancialTransactionService
    {
        /// <summary>
        /// Retrieves a paginated list of financial transactions.
        /// </summary>
        /// <param name="queryObject">
        /// The query parameters used for paging and sorting.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="PagedItems{T}"/>
        /// containing <see cref="FinancialTransactionOutputDto"/> if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<PagedItems<FinancialTransactionOutputDto>>> GetAllAsync(EntityQuery queryObject);

        /// <summary>
        /// Retrieves a financial transaction by its identifier.
        /// </summary>
        /// <param name="id">
        /// The identifier of the financial transaction.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="FinancialTransactionOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<FinancialTransactionOutputDto>> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new financial transaction.
        /// </summary>
        /// <param name="transaction">
        /// The data required to create the financial transaction.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="FinancialTransactionOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<FinancialTransactionOutputDto>> CreateAsync(
            FinancialTransactionCreateInputDto transaction);

        /// <summary>
        /// Updates an existing financial transaction.
        /// </summary>
        /// <param name="id">
        /// The identifier of the financial transaction to update.
        /// </param>
        /// <param name="transaction">
        /// The updated financial transaction data.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="FinancialTransactionOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<FinancialTransactionOutputDto>> UpdateAsync(
            Guid id, FinancialTransactionUpdateInputDto transaction);

        /// <summary>
        /// Deletes a financial transaction.
        /// </summary>
        /// <param name="id">
        /// The identifier of the financial transaction to delete.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="Deleted"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<Deleted>> DeleteAsync(Guid id);

        /// <summary>
        /// Generates a grouped financial transaction report.
        /// </summary>
        /// <param name="queryObject">
        /// The report query containing grouping, paging,
        /// and filtering parameters.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="PagedItems{T}"/>
        /// containing <see cref="GroupedReportOutputDto"/> if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<PagedItems<GroupedReportOutputDto>>> GetReportAsync(ReportQuery queryObject);
    }
}

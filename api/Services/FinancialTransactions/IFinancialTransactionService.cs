using api.Dtos.FinancialTransaction;
using api.Queries;
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
        /// <param name="query">
        /// The query parameters used for paging and sorting.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="PagedItems{T}"/>
        /// containing <see cref="FinancialTransactionOutputDto"/> if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<PagedItems<FinancialTransactionOutputDto>>> GetAllAsync(EntityQuery query);

        /// <summary>
        /// Retrieves a paginated list of financial transactions for administrative purposes.
        /// </summary>
        /// <param name="query">
        /// The query parameters used for paging, sorting, and filtering.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a
        /// <see cref="PagedItems{T}"/> of <see cref="AdminFinancialTransactionOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<PagedItems<AdminFinancialTransactionOutputDto>>> GetAllForAdminAsync(AdminEntityQuery query);

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
        /// Retrieves a financial transaction by its identifier for administrative purposes.
        /// </summary>
        /// <param name="id">
        /// The identifier of the financial transaction.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing an
        /// <see cref="AdminFinancialTransactionOutputDto"/> if successful;
        /// otherwise, an error.
        /// </returns>
        Task<ErrorOr<AdminFinancialTransactionOutputDto>> GetByIdForAdminAsync(Guid id);

        /// <summary>
        /// Creates a new financial transaction.
        /// </summary>
        /// <param name="input">
        /// The data required to create the financial transaction.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="FinancialTransactionOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<FinancialTransactionOutputDto>> CreateAsync(
            FinancialTransactionCreateInputDto input);

        /// <summary>
        /// Creates a new financial transaction as an administrator.
        /// </summary>
        /// <param name="input">
        /// The data required to create the financial transaction.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing an
        /// <see cref="AdminFinancialTransactionOutputDto"/> if successful;
        /// otherwise, an error.
        /// </returns>
        Task<ErrorOr<AdminFinancialTransactionOutputDto>> CreateForAdminAsync(
            AdminFinancialTransactionCreateInputDto input);

        /// <summary>
        /// Updates an existing financial transaction.
        /// </summary>
        /// <param name="id">
        /// The identifier of the financial transaction to update.
        /// </param>
        /// <param name="input">
        /// The updated financial transaction data.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="FinancialTransactionOutputDto"/>
        /// if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<FinancialTransactionOutputDto>> UpdateAsync(
            Guid id, FinancialTransactionUpdateInputDto input);

        /// <summary>
        /// Updates an existing financial transaction as an administrator.
        /// </summary>
        /// <param name="id">
        /// The identifier of the financial transaction to update.
        /// </param>
        /// <param name="input">
        /// The updated financial transaction data.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing an
        /// <see cref="AdminFinancialTransactionOutputDto"/> if successful;
        /// otherwise, an error.
        /// </returns>
        Task<ErrorOr<AdminFinancialTransactionOutputDto>> UpdateForAdminAsync(
            Guid id, FinancialTransactionUpdateInputDto input);

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
        /// <param name="query">
        /// The report query containing grouping, paging,
        /// and filtering parameters.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ErrorOr{T}"/> containing a <see cref="PagedItems{T}"/>
        /// containing <see cref="GroupedReportOutputDto"/> if successful; otherwise, an error.
        /// </returns>
        Task<ErrorOr<PagedItems<GroupedReportOutputDto>>> GetReportAsync(ReportQuery query);
    }
}

using api.Dtos.FinancialTransaction;
using api.Models;
using api.QueryObjects;
using api.Responses;

namespace api.Services.Transaction
{
    /// <summary>
    /// Defines operations for managing financial transactions, including
    /// creation, retrieval, update, deletion, and generating grouped reports.
    /// </summary>
    public interface IFinancialTransactionService
    {
        /// <summary>
        /// Retrieves a paginated list of transactions for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="queryObject">Pagination and sorting parameters.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="PagedData{BaseFinancialTransactionOutputDto}"/> with the transactions.
        /// </returns>
        Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForUserAsync(
            string userId, PaginationQueryObject queryObject);

        /// <summary>
        /// Retrieves a paginated list of transactions for administrative purposes.
        /// </summary>
        /// <param name="queryObject">Pagination and sorting parameters.</param>
        /// <param name="appUserId">Optional filter to restrict results to a specific user, or <see langword="null"/> to include all users.</param>
        /// <returns>A task that returns paged transactions for administration.</returns>
        Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForAdminAsync(
            PaginationQueryObject queryObject, string? appUserId);

        /// <summary>
        /// Retrieves a single transaction by its identifier and maps it to a DTO.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <returns>
        /// A task that returns the transaction DTO if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<BaseFinancialTransactionOutputDto?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a single transaction entity by its identifier without mapping.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <returns>
        /// A task that returns the raw <see cref="FinancialTransaction"/> entity if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<FinancialTransaction?> GetByIdRawAsync(int id);

        /// <summary>
        /// Creates a new transaction on behalf of a specific user by an administrator.
        /// </summary>
        /// <param name="appUserId">The target user identifier.</param>
        /// <param name="transaction">The transaction data transfer object.</param>
        /// <returns>A task that returns the created transaction DTO.</returns>
        Task<BaseFinancialTransactionOutputDto> CreateForAdminAsync(
            string appUserId, AdminFinancialTransactionInputDto transaction);

        /// <summary>
        /// Creates a new transaction for the given user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="transaction">The transaction data transfer object.</param>
        /// <returns>
        /// A task that returns the created transaction DTO if successful; otherwise, <see langword="null"/>.
        /// </returns>
        Task<BaseFinancialTransactionOutputDto?> CreateForUserAsync(
            string userId, BaseFinancialTransactionInputDto transaction);

        /// <summary>
        /// Updates an existing transaction.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <param name="transaction">The transaction update DTO.</param>
        /// <returns>
        /// A task that returns the updated transaction DTO if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<BaseFinancialTransactionOutputDto?> UpdateAsync(
            int id, BaseFinancialTransactionInputDto transaction);

        /// <summary>
        /// Deletes a transaction by its identifier.
        /// </summary>
        /// <param name="id">The transaction identifier.</param>
        /// <returns>
        /// A task that returns <see langword="true"/> if the transaction was deleted; otherwise, <see langword="false"/>.
        /// </returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Generates a grouped report of transactions for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="queryObject">Optional filtering, grouping, and pagination parameters.</param>
        /// <returns>A task that returns a grouped report of transactions.</returns>
        Task<PagedData<GroupedReportOutputDto>> GetReportAsync(string userId, ReportQueryObject queryObject);
    }
}

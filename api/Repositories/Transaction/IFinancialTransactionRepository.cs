using api.Models;

namespace api.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for <see cref="FinancialTransaction"/> entities.
    /// </summary>
    public interface IFinancialTransactionRepository
    {
        /// <summary>
        /// Retrieves all financial transactions asynchronously.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result contains a list of <see cref="FinancialTransaction"/> entities.
        /// </returns>
        Task<List<FinancialTransaction>> GetAllAsync();

        /// <summary>
        /// Retrieves a financial transaction by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the transaction.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result contains the <see cref="FinancialTransaction"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<FinancialTransaction?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new financial transaction asynchronously.
        /// </summary>
        /// <param name="transaction">The transaction entity to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateAsync(FinancialTransaction transaction);

        /// <summary>
        /// Updates an existing financial transaction asynchronously.
        /// </summary>
        /// <param name="transaction">The transaction entity to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(FinancialTransaction transaction);

        /// <summary>
        /// Deletes a financial transaction asynchronously.
        /// </summary>
        /// <param name="transaction">The transaction entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(FinancialTransaction transaction);

        /// <summary>
        /// Returns a queryable collection of financial transactions with their related categories included.
        /// </summary>
        /// <remarks>
        /// Useful for building LINQ queries that require category data without additional joins.
        /// </remarks>
        /// <returns>
        /// A queryable sequence of <see cref="FinancialTransaction"/> entities with categories included.
        /// </returns>
        IQueryable<FinancialTransaction> GetQueryableWithCategory();
    }
}

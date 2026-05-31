using api.Models;
using ErrorOr;

namespace api.Services.Authorization
{
    /// <summary>
    /// Defines authorization checks for accessing financial transactions.
    /// </summary>
    public interface IFinancialTransactionAccessService
    {
        /// <summary>
        /// Verifies whether the current user is authorized
        /// to access the specified financial transaction.
        /// </summary>
        /// <param name="financialTransaction">
        /// The financial transaction to validate access for.
        /// </param>
        /// <returns>
        /// A <see cref="Success"/> result when access is granted;
        /// otherwise, an authorization error.
        /// </returns>
        Task<ErrorOr<Success>> CanAccessCheckAsync(FinancialTransaction financialTransaction);
    }
}

using ErrorOr;

namespace api.Services.UnitOfWork
{
    /// <summary>
    /// Provides a mechanism to execute operations within a transactional scope.
    /// </summary>
    public interface IUnitOfWorkService
    {
        /// <summary>
        /// Executes the specified operation within a database transaction.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="action">
        /// The operation to execute. Returns either a result or an error.
        /// </param>
        /// <returns>
        /// An <see cref="ErrorOr{T}"/> containing either the result or an error.
        /// </returns>
        /// <remarks>
        /// Reuses the current transaction if one is already active; otherwise, creates a new one.
        /// </remarks>
        Task<ErrorOr<T>> ExecuteInTransactionAsync<T>(
            Func<Task<ErrorOr<T>>> action);
    }
}
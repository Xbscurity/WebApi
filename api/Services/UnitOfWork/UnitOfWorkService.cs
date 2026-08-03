using api.Data;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace api.Services.UnitOfWork
{
    /// <summary>
    /// Default implementation of <see cref="IUnitOfWorkService"/>.
    /// </summary>
    public class UnitOfWorkService : IUnitOfWorkService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWorkService"/> class.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ApplicationDbContext"/> used to manage transactions and persist changes.
        /// </param>
        public UnitOfWorkService(ApplicationDbContext context) => _context = context;

        /// <inheritdoc />
        public async Task<ErrorOr<T>> ExecuteInTransactionAsync<T>(
            Func<Task<ErrorOr<T>>> action)
        {
            if (_context.Database.CurrentTransaction != null)
            {
                return await action();
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var result = await action();

                    if (result.IsError)
                    {
                        await transaction.RollbackAsync();
                        return result;
                    }

                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
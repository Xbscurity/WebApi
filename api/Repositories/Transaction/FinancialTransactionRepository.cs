using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Interfaces
{
    /// <summary>
    /// Provides an implementation of <see cref="IFinancialTransactionRepository"/>
    /// for managing financial transactions in the database.
    /// </summary>
    public class FinancialTransactionRepository : IFinancialTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionRepository"/> class.
        /// </summary>
        /// <param name="context">The database context used for data access.</param>
        public FinancialTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<List<FinancialTransaction>> GetAllAsync()
        {
            return await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<FinancialTransaction?> GetByIdAsync(int id)
        {
            return await _context.Transactions.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <inheritdoc />
        public async Task CreateAsync(FinancialTransaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UpdateAsync(FinancialTransaction transaction)
        {
            _context.Update(transaction);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(FinancialTransaction transaction)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public IQueryable<FinancialTransaction> GetQueryableWithCategory()
        {
            return _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.Category.IsActive == true);
        }
    }
}

using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Interfaces
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;


        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FinancialTransaction>> GetAllAsync()
        {
            return await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .ToListAsync();
        }

        public async Task<FinancialTransaction?> GetByIdAsync(int id)
        {
            return await _context.Transactions.Include(t => t.Category).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(FinancialTransaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FinancialTransaction transaction)
        {
            _context.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(FinancialTransaction transaction)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public IQueryable<FinancialTransaction> GetQueryableWithCategory()
        {
            return _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category);
        }
    }
}

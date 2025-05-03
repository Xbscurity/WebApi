using api.Data;
using api.Dtos.FinancialTransaction;
using api.Models;
using api.QueryObjects;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace api.Repositories
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
            return await _context.Transactions.FindAsync(id);
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

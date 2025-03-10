using api.Data;
using api.Helpers.Report;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GroupedReportDto>> GetReportByCategoryAsync(ReportQueryObject? dateRange)
        {
            return await GetGroupedTransactions(
                dateRange,
                t => t.Category == null ? "No category" : t.Category.Name,
                key => new ReportKey { Category = key }
            );
        }

        public async Task<List<GroupedReportDto>> GetReportByDateAsync(ReportQueryObject? dateRange)
        {
            return await GetGroupedTransactions(
                dateRange,
                t => new { t.Date.Month, t.Date.Year },
                key => new ReportKey { Month = key.Month, Year = key.Year }
            );
        }

        public async Task<List<GroupedReportDto>> GetReportByCategoryAndDateAsync(ReportQueryObject? dateRange)
        {
            return await GetGroupedTransactions(
                dateRange,
                t => new { Category = t.Category.Name ?? "No category", t.Date.Month, t.Date.Year },
                key => new ReportKey { Category = key.Category, Month = key.Month, Year = key.Year }
            );
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

        public async Task<FinancialTransaction> CreateAsync(FinancialTransaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<bool> UpdateAsync(int id, FinancialTransaction transaction)
        {
            var existingTransaction = await _context.Transactions.FindAsync(id);
            if (existingTransaction == null)
            {
                return false;
            }

            existingTransaction.Amount = transaction.Amount;
            existingTransaction.CategoryId = transaction.CategoryId;
            existingTransaction.Comment = transaction.Comment;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return false;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<FinancialTransaction> GetFilteredTransactions(ReportQueryObject? dateRange)
        {
            var transactions = _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .AsQueryable();

            if (dateRange?.StartDate != null)
            {
                transactions = transactions.Where(t => t.Date >= dateRange.StartDate);
            }
            if (dateRange?.EndDate != null)
            {
                transactions = transactions.Where(t => t.Date <= dateRange.EndDate);
            }

            return transactions;
        }

        private async Task<List<GroupedReportDto>> GetGroupedTransactions<TGroupKey>(
            ReportQueryObject? dateRange,
            Expression<Func<FinancialTransaction, TGroupKey>> groupBySelector,
            Func<TGroupKey, ReportKey> keySelector)
        {
            var transactions = GetFilteredTransactions(dateRange);

            return await transactions
                .GroupBy(groupBySelector)
                .Select(group => new GroupedReportDto
                {
                    Key = keySelector(group.Key),
                    Transactions = group.Select(transaction => new ReportTransactionDto
                    {
                        Category = transaction.Category.Name ?? "No category",
                        Date = transaction.Date,
                        Amount = transaction.Amount,
                        Comment = transaction.Comment
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}

using api.Data;
using api.Helpers;
using api.Helpers.Report;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace api.Repositories
{
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionsRepository(ApplicationDbContext context)
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
                t => new { t.CreatedAt.Month, t.CreatedAt.Year },
                key => new ReportKey { Month = key.Month, Year = key.Year }
            );
        }

        public async Task<List<GroupedReportDto>> GetReportByCategoryAndDateAsync(ReportQueryObject? dateRange)
        {
            return await GetGroupedTransactions(
                dateRange,
                t => new { Category = t.Category.Name ?? "No category", t.CreatedAt.Month, t.CreatedAt.Year },
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

        private IQueryable<FinancialTransaction> GetFilteredTransactions(ReportQueryObject? dateRange)
        {
            var transactions = _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .AsQueryable();

            if (dateRange?.StartDate != null)
            {
                transactions = transactions.Where(t => t.CreatedAt >= dateRange.StartDate);
            }
            if (dateRange?.EndDate != null)
            {
                transactions = transactions.Where(t => t.CreatedAt <= dateRange.EndDate);
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
                        CreatedAt = transaction.CreatedAt,
                        Amount = transaction.Amount,
                        Comment = transaction.Comment
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}

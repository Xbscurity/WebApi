using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Enums;
using api.Models;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction
{
    public class GroupByDateStrategy : IGroupingReportStrategy
    {
        public GroupingReportStrategyKey Key => GroupingReportStrategyKey.ByDate;

        public async Task<List<GroupedReportDto>> GroupAsync(
            IQueryable<FinancialTransaction> transactions)
        {
            return await transactions
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(group => new GroupedReportDto
                {
                    Key = new ReportKey
                    {
                        Year = group.Key.Year,
                        Month = group.Key.Month
                    },
                    Transactions = group.Select(transaction => new FinancialTransactionOutputDto
                    (
                        transaction.Id,
                        transaction.Category == null ? "No category" : transaction.Category.Name,
                        transaction.Amount,
                        transaction.Comment,
                        transaction.CreatedAt
                    )).ToList()
                })
                .ToListAsync();
        }
    }
}

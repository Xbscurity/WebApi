using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction
{
    /// <summary>
    /// Groups transactions by year and month of creation.
    /// </summary>
    public class GroupByDateStrategy : IGroupingReportStrategy
    {
        /// <inheritdoc/>
        public GroupingReportStrategyKey Key => GroupingReportStrategyKey.ByDate;

        /// <inheritdoc/>
        public async Task<List<GroupedReportOutputDto>> GroupAsync(
            IQueryable<FinancialTransaction> transactions)
        {
            return await transactions
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(group => new GroupedReportOutputDto
                {
                    Key = new ReportKey
                    {
                        Year = group.Key.Year,
                        Month = group.Key.Month,
                    },
                    Transactions = group.Select(transaction => new BaseFinancialTransactionOutputDto()
                    {
                        Id = transaction.Id,
                        CategoryName = transaction.Category == null ? "No category" : transaction.Category.Name,
                        Amount = transaction.Amount,
                        Comment = transaction.Comment,
                        CreatedAt = transaction.CreatedAt,
                        AppUserId = transaction.AppUserId,
                    }).ToList(),
                })
                .ToListAsync();
        }
    }
}

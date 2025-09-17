using api.Dtos.FinancialTransaction;
using api.Enums;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction
{
    /// <summary>
    /// Groups transactions by both creation date (year, month) and category.
    /// </summary>
    public class GroupByDateAndCategoryStrategy : IGroupingReportStrategy
    {
        /// <inheritdoc/>
        public GroupingReportStrategyKey Key => GroupingReportStrategyKey.ByCategoryAndDate;

        /// <inheritdoc/>
        public async Task<List<GroupedReportOutputDto>> GroupAsync(
           IQueryable<Models.FinancialTransaction> transactions)
        {
            return await transactions
                .GroupBy(transactions => new
                {
                    transactions.CreatedAt.Year,
                    transactions.CreatedAt.Month,
                    Category = transactions.Category == null ? "No category" : transactions.Category.Name.Trim(),
                })
                .Select(group => new GroupedReportOutputDto
                {
                    Key = new ReportKey
                    {
                        Year = group.Key.Year,
                        Month = group.Key.Month,
                        Category = group.Key.Category,
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

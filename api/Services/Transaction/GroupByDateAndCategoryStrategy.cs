using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Enums;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction
{
    public class GroupByDateAndCategoryStrategy : IGroupingReportStrategy
    {
        public GroupingReportStrategyKey Key => GroupingReportStrategyKey.ByCategoryAndDate;

        public async Task<List<GroupedReportDto>> GroupAsync(
           IQueryable<Models.FinancialTransaction> transactions)
        {
            return await transactions
                .GroupBy(transactions => new
                {
                    transactions.CreatedAt.Year,
                    transactions.CreatedAt.Month,
                    Category = transactions.Category == null ? "No category" : transactions.Category.Name.Trim(),
                })
                .Select(group => new GroupedReportDto
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

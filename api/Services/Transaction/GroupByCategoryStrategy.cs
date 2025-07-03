using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Enums;
using api.Models;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction
{
    public class GroupByCategoryStrategy : IGroupingReportStrategy
    {
        public GroupingReportStrategyKey Key => GroupingReportStrategyKey.ByCategory;

        public async Task<List<GroupedReportDto>> GroupAsync(
            IQueryable<FinancialTransaction> transactions)
        {
            return await transactions
                .GroupBy(t =>
                    t.Category == null ? "No category" : t.Category.Name.Trim())
                .Select(group => new GroupedReportDto
                {
                    Key = new ReportKey
                    {
                        Category = group.Key,
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

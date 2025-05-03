using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Enums;
using api.Models;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction
{
    public class GroupByDateAndCategoryStrategy : IGroupingReportStrategy
    {
        public GroupingReportStrategyKey Key => GroupingReportStrategyKey.ByCategoryAndDate;
        public async Task<List<GroupedReportDto>> GroupAsync(
           IQueryable<FinancialTransaction> transactions)
        {
            return await transactions
                .GroupBy(transactions => new
                {
                    transactions.CreatedAt.Year,
                    transactions.CreatedAt.Month,
                    Category = transactions.Category == null ? "No category" : transactions.Category.Name,
                }
                )
                .Select(group => new GroupedReportDto
                {
                    Key = new ReportKey
                    {
                        Year = group.Key.Year,
                        Month = group.Key.Month,
                        Category = group.Key.Category
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

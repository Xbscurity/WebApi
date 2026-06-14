using api.Data;
using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Models;
using api.Queries;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace api.Interfaces
{
    /// <summary>
    /// Default implementation of <see cref="IFinancialTransactionRepository"/>.
    /// </summary>
    public class FinancialTransactionRepository : RepositoryBase<FinancialTransaction>, IFinancialTransactionRepository
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="FinancialTransactionRepository"/> class.
        /// </summary>
        /// <param name="context">
        /// The database context used for persistence operations.
        /// </param>
        public FinancialTransactionRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<List<GroupedReportOutputDto>> GetGroupedListByCategory(ISpecification<FinancialTransaction> spec, ReportQuery query)
            => await GetGroupedListAsync(
                spec, query, t => t.Category.Name, key => new ReportKey.CategoryKey(key));

        /// <inheritdoc />
        public async Task<List<GroupedReportOutputDto>> GetGroupedListByDate(ISpecification<FinancialTransaction> spec, ReportQuery query)
            => await GetGroupedListAsync(
                spec, query, t => t.CreatedAt, key => new ReportKey.DateKey(key.Year, key.Month));

        /// <inheritdoc />
        public async Task<List<GroupedReportOutputDto>> GetGroupedListByCategoryAndDate(
            ISpecification<FinancialTransaction> spec, ReportQuery query)
            => await GetGroupedListAsync(
                spec,
                query,
                t => new { t.CreatedAt, t.Category.Name },
                key => new ReportKey.CategoryAndDateKey(key.Name, key.CreatedAt.Year, key.CreatedAt.Month));

        /// <summary>
        /// Retrieves grouped financial transaction data using the specified grouping key.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the grouping key.
        /// </typeparam>
        /// <param name="spec">
        /// The specification used to filter financial transactions.
        /// </param>
        /// <param name="query">
        /// The report query containing paging parameters.
        /// </param>
        /// <param name="groupBy">
        /// The expression used to group transactions.
        /// </param>
        /// <param name="reportKeyFactory">
        /// The mapper used to transform the grouping key
        /// into a <see cref="ReportKey"/>.
        /// </param>
        /// <returns>
        /// A collection of grouped financial transaction report results.
        /// </returns>
        /// <remarks>
        /// Each group includes:
        /// <list type="bullet">
        /// <item>
        /// <description>Total transaction amount.</description>
        /// </item>
        /// <item>
        /// <description>Total transaction count.</description>
        /// </item>
        /// <item>
        /// <description>
        /// Up to 10 transactions ordered by descending amount.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        private async Task<List<GroupedReportOutputDto>> GetGroupedListAsync<TKey>(
            ISpecification<FinancialTransaction> spec,
            ReportQuery query,
            Expression<Func<FinancialTransaction, TKey>> groupBy,
            Func<TKey, ReportKey> reportKeyFactory)
        {
            var filteredQuery = ApplySpecification(spec);
            var groupedQuery = await filteredQuery.GroupBy(groupBy).
                Select(group => new
                {
                    group.Key,
                    Count = group.Count(),
                    TotalAmount = group.Sum(t => t.Type == FinancialTransactionType.Income ? t.Amount : -t.Amount),
                    Transactions = group.OrderByDescending(t => t.Amount).Take(10).Select(transaction => new
                    {
                        transaction.Id,
                        transaction.Amount,
                        transaction.Type,
                        transaction.Comment,
                        transaction.CreatedAt,
                        transaction.UpdatedAt,
                        transaction.AppUserId,
                        transaction.CategoryId,
                    }),
                }).OrderByDescending(g => Math.Abs(g.TotalAmount))
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size).ToListAsync();

            return groupedQuery.Select(r => new GroupedReportOutputDto
            {
                GroupKey = reportKeyFactory(r.Key),
                Count = r.Count,
                TotalAmount = r.TotalAmount,
                Transactions = r.Transactions
            .Select(t => new FinancialTransactionOutputDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type,
                Comment = t.Comment,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CategoryId = t.CategoryId,
            })
            .ToList(),
            }).ToList();
        }
    }
}
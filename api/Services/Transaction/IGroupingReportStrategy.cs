using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Models;

namespace api.Services.Transaction
{
    public interface IGroupingReportStrategy
    {
        GroupingReportStrategyKey Key { get; }

        Task<List<GroupedReportDto>> GroupAsync(IQueryable<FinancialTransaction> queryable);
    }
}

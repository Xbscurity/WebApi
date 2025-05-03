using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Models;

namespace api.Services.Interfaces
{
    public interface IGroupingReportStrategy
    {
        GroupingReportStrategyKey Key { get; }
        Task<List<GroupedReportDto>> GroupAsync(IQueryable<FinancialTransaction> queryable);
    }
}

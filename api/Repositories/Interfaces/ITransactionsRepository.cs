using api.Helpers;
using api.Helpers.Report;
using api.Models;

namespace api.Repositories.Interfaces
{
    public interface ITransactionsRepository
    {
        Task<List<GroupedReportDto>> GetReportByCategoryAsync(ReportQueryObject? dateRange);
        Task<List<GroupedReportDto>> GetReportByDateAsync(ReportQueryObject? dateRange);
        Task<List<GroupedReportDto>> GetReportByCategoryAndDateAsync(ReportQueryObject? dateRange);
        Task<List<FinancialTransaction>> GetAllAsync();
        Task<FinancialTransaction?> GetByIdAsync(int id);
        Task CreateAsync(FinancialTransaction transaction);
        Task UpdateAsync(FinancialTransaction transaction);
        Task DeleteAsync(FinancialTransaction transaction);
    }
}

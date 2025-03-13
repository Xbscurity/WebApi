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
        Task<FinancialTransaction> CreateAsync(FinancialTransaction transaction);
        Task<bool> UpdateAsync(int id, FinancialTransaction transaction);
        Task<bool> DeleteAsync(int id);
    }
}

using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Helpers;
using api.QueryObjects;

namespace api.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedData<FinancialTransactionOutputDto>> GetAllAsync(PaginationQueryObject queryObject);

        Task<FinancialTransactionOutputDto?> GetByIdAsync(int id);

        Task<FinancialTransactionOutputDto> CreateAsync(FinancialTransactionInputDto transaction);

        Task<FinancialTransactionOutputDto?> UpdateAsync(int id, FinancialTransactionInputDto transaction);

        Task<bool> DeleteAsync(int id);

        Task<PagedData<GroupedReportDto>> GetReportAsync(ReportQueryObject? queryObject);
    }
}

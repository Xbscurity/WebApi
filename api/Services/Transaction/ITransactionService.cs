using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.QueryObjects;
using api.Responses;
using api.Services.Common;

namespace api.Services.Transaction
{
    public interface ITransactionService
    {
        Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForUserAsync(CurrentUser user, PaginationQueryObject queryObject);

        Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForAdminAsync(PaginationQueryObject queryObject, string? appUserId);

        Task<BaseFinancialTransactionOutputDto?> GetByIdAsync(CurrentUser user, int id);

        Task<BaseFinancialTransactionOutputDto> CreateForAdminAsync(AdminFinancialTransactionCreateInputDto transaction, string appUserId);

        Task<BaseFinancialTransactionOutputDto> CreateForUserAsync(CurrentUser user, BaseFinancialTransactionInputDto transaction);

        Task<BaseFinancialTransactionOutputDto?> UpdateAsync(CurrentUser user, int id, BaseFinancialTransactionInputDto transaction);

        Task<bool> DeleteAsync(CurrentUser user, int id);

        Task<PagedData<GroupedReportDto>> GetReportAsync(CurrentUser user, ReportQueryObject? queryObject);
    }
}

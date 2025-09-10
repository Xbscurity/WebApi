using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Models;
using api.QueryObjects;
using api.Responses;

namespace api.Services.Transaction
{
    public interface ITransactionService
    {
        Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForUserAsync(
            string userId, PaginationQueryObject queryObject);

        Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForAdminAsync(
            PaginationQueryObject queryObject, string? appUserId);

        Task<BaseFinancialTransactionOutputDto?> GetByIdAsync(int id);

        Task<FinancialTransaction?> GetByIdRawAsync(int id);

        Task<BaseFinancialTransactionOutputDto> CreateForAdminAsync(
            string appUserId, AdminFinancialTransactionCreateInputDto transaction);

        Task<BaseFinancialTransactionOutputDto> CreateForUserAsync(
            string userId, BaseFinancialTransactionInputDto transaction);

        Task<BaseFinancialTransactionOutputDto?> UpdateAsync(
            int id, BaseFinancialTransactionInputDto transaction);

        Task<bool> DeleteAsync(int id);

        Task<PagedData<GroupedReportDto>> GetReportAsync(string userId, ReportQueryObject? queryObject);
    }
}

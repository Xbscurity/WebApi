using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Helpers;
using api.QueryObjects;
using System.Security.Claims;

namespace api.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForUserAsync(ClaimsPrincipal user, PaginationQueryObject queryObject);

        Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForAdminAsync(PaginationQueryObject queryObject, string? appUserId = null);

        Task<BaseFinancialTransactionOutputDto?> GetByIdAsync(ClaimsPrincipal user, int id);

        Task<BaseFinancialTransactionOutputDto> CreateForAdminAsync(ClaimsPrincipal user, AdminFinancialTransactionInputDto transaction);

        Task<BaseFinancialTransactionOutputDto> CreateForUserAsync(ClaimsPrincipal user, BaseFinancialTransactionInputDto transaction);

        Task<BaseFinancialTransactionOutputDto?> UpdateAsync(ClaimsPrincipal user, int id, BaseFinancialTransactionInputDto transaction);

        Task<bool> DeleteAsync(ClaimsPrincipal user, int id);

        Task<PagedData<GroupedReportDto>> GetReportAsync(ClaimsPrincipal user, ReportQueryObject? queryObject);
    }
}

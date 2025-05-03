using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Enums;
using api.Models;
using api.QueryObjects;
using Microsoft.AspNetCore.Mvc;

namespace api.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<List<FinancialTransactionOutputDto>> GetAllAsync();
        Task<FinancialTransactionOutputDto?> GetByIdAsync(int id);
        Task<FinancialTransactionOutputDto> CreateAsync(FinancialTransactionInputDto transaction);
        Task<FinancialTransactionOutputDto?> UpdateAsync(int id, FinancialTransactionInputDto transaction);
        Task<bool> DeleteAsync(int id);
        Task<List<GroupedReportDto>> GetReportAsync(ReportQueryObject? dateRange, GroupingReportStrategyKey reportType);
    }
}

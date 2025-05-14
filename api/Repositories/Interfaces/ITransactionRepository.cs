﻿using api.Models;

namespace api.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<List<FinancialTransaction>> GetAllAsync();
        Task<FinancialTransaction?> GetByIdAsync(int id);
        Task CreateAsync(FinancialTransaction transaction);
        Task UpdateAsync(FinancialTransaction transaction);
        Task DeleteAsync(FinancialTransaction transaction);
        IQueryable<FinancialTransaction> GetQueryableWithCategory();
    }
}

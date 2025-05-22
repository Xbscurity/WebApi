using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Enums;
using api.Extensions;
using api.Helpers;
using api.Providers.Interfaces;
using api.QueryObjects;
using api.Repositories.Interfaces;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly Dictionary<GroupingReportStrategyKey, IGroupingReportStrategy> _strategies;
        public TransactionService(
            ITransactionRepository transactionsRepository,
            ITimeProvider timeProvider,
            IEnumerable<IGroupingReportStrategy> strategies)
        {
            _transactionRepository = transactionsRepository;
            _timeProvider = timeProvider;
            _strategies = strategies.ToDictionary(s => s.Key);
        }

        public async Task<FinancialTransactionOutputDto> CreateAsync(FinancialTransactionInputDto transactionDto)
        {
            var transaction = transactionDto.ToModel(_timeProvider);
            await _transactionRepository.CreateAsync(transaction);
            return transaction.ToOutputDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);
            if (existingTransaction is null)
            {
                return false;
            }

            await _transactionRepository.DeleteAsync(existingTransaction);
            return true;
        }

        public async Task<PagedData<FinancialTransactionOutputDto>> GetAllAsync(PaginationQueryObject queryObject)
        {
            var query = _transactionRepository.GetQueryableWithCategory();
            var result = await query
                .ApplySorting(queryObject)
                .Select(t => t.ToOutputDto())
                .ToPagedQueryAsync(queryObject);
            return new PagedData<FinancialTransactionOutputDto>
            {
                Data = await result.Query.ToListAsync(),
                Pagination = result.Pagination,
            };
        }

        public async Task<FinancialTransactionOutputDto?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            return transaction.ToOutputDto();
        }

        public async Task<PagedData<GroupedReportDto>> GetReportAsync(ReportQueryObject queryObject)
        {
            var strategy = _strategies[queryObject.Key];
            var query = _transactionRepository.GetQueryableWithCategory();
            var transactions = await query
                .ApplyFiltering(queryObject)
                .ApplySorting(queryObject)
                .ToPagedQueryAsync(queryObject);
            return new PagedData<GroupedReportDto>
            {
                Data = await strategy.GroupAsync(transactions.Query),
                Pagination = transactions.Pagination,
            };
        }

        public async Task<FinancialTransactionOutputDto?> UpdateAsync(int id, FinancialTransactionInputDto transaction)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);
            if (existingTransaction is null)
            {
                return null;
            }
            existingTransaction.CategoryId = transaction.CategoryId;
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.Comment = transaction.Comment;
            await _transactionRepository.UpdateAsync(existingTransaction);
            return existingTransaction.ToOutputDto();
        }
    }
}

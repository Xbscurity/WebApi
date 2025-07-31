using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Enums;
using api.Extensions;
using api.Helpers;
using api.Models;
using api.Providers.Interfaces;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api.Services.Transaction
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly Dictionary<GroupingReportStrategyKey, IGroupingReportStrategy> _strategies;

        public TransactionService(
            ITransactionRepository transactionsRepository,
            ITimeProvider timeProvider,
            IEnumerable<IGroupingReportStrategy> strategies,
            ICategoryRepository categoryRepository)
        {
            _transactionRepository = transactionsRepository;
            _timeProvider = timeProvider;
            _strategies = strategies.ToDictionary(s => s.Key);
            _categoryRepository = categoryRepository;
        }

        public async Task<BaseFinancialTransactionOutputDto> CreateForAdminAsync(ClaimsPrincipal user, AdminFinancialTransactionInputDto transactionDto)
        {
            var transaction = transactionDto.ToModel(_timeProvider);
            await _transactionRepository.CreateAsync(transaction);
            return transaction.ToOutputDto();
        }

        public async Task<BaseFinancialTransactionOutputDto?> CreateForUserAsync(ClaimsPrincipal user, BaseFinancialTransactionInputDto transactionDto)
        {
            var category = await _categoryRepository.GetByIdAsync(transactionDto.CategoryId);
            if (category == null || category.IsActive == false)
            {
                return null;
            }

            if (category.AppUserId != null && category.AppUserId != user.GetUserId())
            {
                return null;
            }

            var transaction = transactionDto.ToModel(user.GetUserId(), _timeProvider);
            await _transactionRepository.CreateAsync(transaction);
            return transaction.ToOutputDto();
        }

        public async Task<bool> DeleteAsync(ClaimsPrincipal user, int id)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);
            if (existingTransaction is null)
            {
                return false;
            }
            if (!HasAccess(user, existingTransaction))
            {
                return false;
            }

            await _transactionRepository.DeleteAsync(existingTransaction);
            return true;
        }

        public async Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForAdminAsync(PaginationQueryObject queryObject, string? appUserId = null)
        {
            var query = _transactionRepository.GetQueryableWithCategory();
            var sortedTransactionDtos = query
                .ApplySorting(queryObject)
                .Select(t => t.ToOutputDto());
            if (appUserId != null)
            {
                sortedTransactionDtos = sortedTransactionDtos.Where(t => t.AppUserId == appUserId);
            }

            var result = await sortedTransactionDtos.ToPagedQueryAsync(queryObject);
            return new PagedData<BaseFinancialTransactionOutputDto>
            {
                Data = await result.Query.ToListAsync(),
                Pagination = result.Pagination,
            };
        }

        public async Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForUserAsync(ClaimsPrincipal user, PaginationQueryObject queryObject)
        {
            var query = _transactionRepository.GetQueryableWithCategory();
            var result = await query
                .ApplySorting(queryObject)
                .Where(t => t.AppUserId == user.GetUserId())
                .Select(t => t.ToOutputDto())
                .ToPagedQueryAsync(queryObject);
            return new PagedData<BaseFinancialTransactionOutputDto>
            {
                Data = await result.Query.ToListAsync(),
                Pagination = result.Pagination,
            };
        }

        public async Task<BaseFinancialTransactionOutputDto?> GetByIdAsync(ClaimsPrincipal user, int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (!HasAccess(user, transaction))
            {
                return null;
            }

            return transaction.ToOutputDto();
        }

        public async Task<PagedData<GroupedReportDto>> GetReportAsync(ClaimsPrincipal user, ReportQueryObject queryObject)
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

        public async Task<BaseFinancialTransactionOutputDto?> UpdateAsync(ClaimsPrincipal user, int id, BaseFinancialTransactionInputDto transactionDto)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);
            if (existingTransaction is null)
            {
                return null;
            }

            if (!HasAccess(user, existingTransaction))
            {
                return null;
            }

            existingTransaction.CategoryId = transactionDto.CategoryId;
            existingTransaction.Amount = transactionDto.Amount;
            existingTransaction.Comment = transactionDto.Comment.Trim();
            await _transactionRepository.UpdateAsync(existingTransaction);
            return existingTransaction.ToOutputDto();
        }

        private bool HasAccess(ClaimsPrincipal user, FinancialTransaction transaction)
        {
            var isAdmin = user.IsInRole(Roles.Admin);
            return isAdmin || transaction.AppUserId == user.GetUserId() || transaction.AppUserId == null;
        }
    }
}

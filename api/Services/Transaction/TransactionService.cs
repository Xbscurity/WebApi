﻿using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Dtos.FinancialTransactions;
using api.Enums;
using api.Extensions;
using api.Models;
using api.Providers.Interfaces;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Repositories.Interfaces;
using api.Responses;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly Dictionary<GroupingReportStrategyKey, IGroupingReportStrategy> _strategies;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionsRepository,
            ITimeProvider timeProvider,
            IEnumerable<IGroupingReportStrategy> strategies,
            ICategoryRepository categoryRepository,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionsRepository;
            _timeProvider = timeProvider;
            _strategies = strategies.ToDictionary(s => s.Key);
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<BaseFinancialTransactionOutputDto> CreateForAdminAsync(
            string userId, AdminFinancialTransactionCreateInputDto transactionDto)
        {
            var transaction = transactionDto.ToModel(_timeProvider);
            await _transactionRepository.CreateAsync(transaction);
            return transaction.ToOutputDto();
        }

        public async Task<BaseFinancialTransactionOutputDto?> CreateForUserAsync(
            string userId, BaseFinancialTransactionInputDto transactionDto)
        {
            var category = await _categoryRepository.GetByIdAsync(transactionDto.CategoryId);
            if (category == null || category.IsActive == false)
            {
                _logger.LogDebug("Category {CategoryId} not found or inactive", transactionDto.CategoryId);
                return null;
            }

            if (category.AppUserId != null && category.AppUserId != userId)
            {
                _logger.LogWarning(
                    LoggingEvents.Categories.Common.NoAccess,
                    "Access denied to category {CategoryId}",
                    transactionDto.CategoryId);
                return null;
            }

            var transaction = transactionDto.ToModel(userId!, _timeProvider);
            await _transactionRepository.CreateAsync(transaction);
            return transaction.ToOutputDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);
            if (existingTransaction is null)
            {
                _logger.LogDebug("Transaction {TransactionId} not found", id);
                return false;
            }

            await _transactionRepository.DeleteAsync(existingTransaction);
            return true;
        }

        public async Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForAdminAsync(
            PaginationQueryObject queryObject, string? appUserId)
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

        public async Task<PagedData<BaseFinancialTransactionOutputDto>> GetAllForUserAsync(
            string userId, PaginationQueryObject queryObject)
        {
            var query = _transactionRepository.GetQueryableWithCategory();
            var result = await query
                .Where(t => t.AppUserId == userId)
                .ApplySorting(queryObject)
                .Select(t => t.ToOutputDto())
                .ToPagedQueryAsync(queryObject);
            return new PagedData<BaseFinancialTransactionOutputDto>
            {
                Data = await result.Query.ToListAsync(),
                Pagination = result.Pagination,
            };
        }

        public async Task<BaseFinancialTransactionOutputDto?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                _logger.LogDebug("Transaction {TransactionId} not found", id);
                return null;
            }
            return transaction.ToOutputDto();
        }

        public async Task<FinancialTransaction?> GetByIdRawAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                _logger.LogDebug("Transaction {TransactionId} not found", id);
                return null;
            }

            return transaction;
        }

        public async Task<PagedData<GroupedReportDto>> GetReportAsync(string userId, ReportQueryObject queryObject)
        {
            var strategy = _strategies[queryObject.Key];
            var query = _transactionRepository.GetQueryableWithCategory();
            var transactions = await query
                .Where(t => t.AppUserId == userId)
                .ApplyFiltering(queryObject)
                .ApplySorting(queryObject)
                .ToPagedQueryAsync(queryObject);
            return new PagedData<GroupedReportDto>
            {
                Data = await strategy.GroupAsync(transactions.Query),
                Pagination = transactions.Pagination,
            };
        }

        public async Task<BaseFinancialTransactionOutputDto?> UpdateAsync(
            int id, BaseFinancialTransactionInputDto transactionDto)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);
            if (existingTransaction is null)
            {
                _logger.LogDebug("Transaction {TransactionId} not found", id);
                return null;
            }

            existingTransaction.CategoryId = transactionDto.CategoryId;
            existingTransaction.Amount = transactionDto.Amount;
            existingTransaction.Comment = transactionDto.Comment.Trim();
            await _transactionRepository.UpdateAsync(existingTransaction);
            return existingTransaction.ToOutputDto();
        }
    }
}

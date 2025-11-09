using api.Constants;
using api.Dtos.FinancialTransaction;
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
    /// <summary>
    /// Provides an implementation of <see cref="IFinancialTransactionService"/> that uses repositories
    /// and grouping strategies to manage financial transactions and reports.
    /// </summary>
    public class FinancialTransactionService : IFinancialTransactionService
    {
        private readonly IFinancialTransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly Dictionary<GroupingReportStrategyKey, IGroupingReportStrategy> _strategies;
        private readonly ILogger<FinancialTransactionService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionService"/> class.
        /// </summary>
        /// <param name="transactionsRepository">The repository for transactions.</param>
        /// <param name="timeProvider">The time provider for generating timestamps.</param>
        /// <param name="strategies">The available grouping strategies for reports.</param>
        /// <param name="categoryRepository">The repository for categories.</param>
        /// <param name="logger">The logger for audit and debugging information.</param>
        public FinancialTransactionService(
            IFinancialTransactionRepository transactionsRepository,
            ICategoryRepository categoryRepository,
            ILogger<FinancialTransactionService> logger,
            ITimeProvider timeProvider,
            IEnumerable<IGroupingReportStrategy> strategies)
        {
            _transactionRepository = transactionsRepository;
            _timeProvider = timeProvider;
            _strategies = strategies.ToDictionary(s => s.Key);
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<BaseFinancialTransactionMinimalOutputDto> CreateForAdminAsync(
            AdminFinancialTransactionInputDto transactionDto)
        {
            var transaction = transactionDto.ToModel(_timeProvider);

            await _transactionRepository.CreateAsync(transaction);

            _logger.LogInformation(
                LoggingEvents.Categories.Admin.Created,
                "Created new transaction {transactionId} for user {UserId}",
                transaction.Id,
                transaction.AppUserId);

            return transaction.ToMinimalOutputDto();
        }

        /// <inheritdoc/>
        public async Task<BaseFinancialTransactionMinimalOutputDto?> CreateForUserAsync(
            string userId, BaseFinancialTransactionInputDto transactionDto)
        {
            var transaction = transactionDto.ToModel(userId!, _timeProvider);

            await _transactionRepository.CreateAsync(transaction);

            _logger.LogInformation(
                LoggingEvents.FinancialTransactions.User.Created,
                "Created new transaction {transactionId}",
                transaction.Id);
            return transaction.ToMinimalOutputDto();
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);

            _logger.LogInformation(
                LoggingEvents.FinancialTransactions.Common.Deleted,
                "Financial transaction with ID {FinancialTransactionId} deleted.",
                id);
            await _transactionRepository.DeleteAsync(existingTransaction!);
            return true;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<BaseFinancialTransactionOutputDto?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);

            _logger.LogInformation(
                LoggingEvents.FinancialTransactions.Common.GetById,
                "Financial transaction with ID {FinancialTransactionId} retrieved.",
                id);

            return transaction!.ToOutputDto();
        }

        /// <inheritdoc/>
        public async Task<PagedData<GroupedReportOutputDto>> GetReportAsync(string userId, ReportQueryObject queryObject)
        {
            var strategy = _strategies[queryObject.Key];

            var query = _transactionRepository.GetQueryableWithCategory();

            var transactions = await query
                .Where(t => t.AppUserId == userId)
                .ApplyFiltering(queryObject)
                .ApplySorting(queryObject)
                .ToPagedQueryAsync(queryObject);

            return new PagedData<GroupedReportOutputDto>
            {
                Data = await strategy.GroupAsync(transactions.Query),
                Pagination = transactions.Pagination,
            };
        }

        /// <inheritdoc/>
        public async Task<BaseFinancialTransactionOutputDto?> UpdateAsync(
            int id, BaseFinancialTransactionInputDto transactionDto)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);

            var existingCategorytransactionDto = await _categoryRepository.GetByIdAsync(transactionDto.CategoryId);

            existingTransaction!.CategoryId = transactionDto.CategoryId;
            existingTransaction.Amount = transactionDto.Amount;
            existingTransaction.Comment = transactionDto.Comment.Trim();

            await _transactionRepository.UpdateAsync(existingTransaction);

            _logger.LogInformation(
                LoggingEvents.FinancialTransactions.Common.Updated,
                "Financial transaction with ID {FinancialTransactionId} updated.",
                id);

            return existingTransaction.ToOutputDto();
        }
    }
}

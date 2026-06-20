using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Providers.CurrentUser;
using api.Queries;
using api.Services.Shared;
using api.Specifications;
using ErrorOr;
using System.Collections.Frozen;

namespace api.Services.FinancialTransactions
{
    /// <summary>
    /// Default implementation of <see cref="IFinancialTransactionService"/>.
    /// </summary>
    public class FinancialTransactionService : IFinancialTransactionService
    {
        private static readonly FrozenSet<string> ValidFields = new[]
        {
            "id",
            "category",
            "amount",
            "date",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<FinancialTransactionService> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IFinancialTransactionRepository _financialTransactionRepository;
        private readonly Dictionary<GroupingReportStrategyKey, IGroupingReportStrategy> _strategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger used for diagnostic and audit logging.
        /// </param>
        /// <param name="currentUser">
        /// The current authenticated user context.
        /// </param>
        /// <param name="categoryRepository">
        /// The repository used to access category data.
        /// </param>
        /// <param name="financialTransactionsRepository">
        /// The repository used to manage financial transaction persistence.
        /// </param>
        /// <param name="strategies">
        /// The available grouping strategies for report generation.
        /// </param>
        public FinancialTransactionService(
            ILogger<FinancialTransactionService> logger,
            ICurrentUser currentUser,
            IRepository<Category> categoryRepository,
            IFinancialTransactionRepository financialTransactionsRepository,
            IEnumerable<IGroupingReportStrategy> strategies)
        {
            _logger = logger;
            _currentUser = currentUser;
            _categoryRepository = categoryRepository;
            _financialTransactionRepository = financialTransactionsRepository;
            _strategies = strategies.ToDictionary(s => s.Key);
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<PagedItems<FinancialTransactionOutputDto>>> GetAllAsync(
            EntityQuery query)
        {
            if (!ValidFields.Contains(query.SortBy))
            {
                var allowed = string.Join(", ", ValidFields);

                _logger.LogWarning(
                    LoggingEvents.FinancialTransaction.SortInvalid,
                    "SortBy '{Field}' is invalid. Allowed fields: {AllowedFields}",
                    query.SortBy,
                    allowed);

                return Errors.FT.InvalidSortBy(query.SortBy, ValidFields);
            }

            var spec = new FinancialTransactionSortedPagedSpecification(query, _currentUser);
            var financialTransactions = await _financialTransactionRepository.ListAsync(spec);

            var count = await _financialTransactionRepository.CountAsync(spec);

            var pagination = new Pagination(query.Page, query.Size, count);

            var pagedData = new PagedItems<FinancialTransactionOutputDto>
            {
                Items = financialTransactions,
                Pagination = pagination,
            };

            _logger.LogInformation(
               "Returning {Count} financial transactions. Page={PageNumber}, Size={PageSize}, SortBy={SortBy}",
               pagedData.Items.Count,
               pagedData.Pagination.PageNumber,
               pagedData.Pagination.PageSize,
               query.SortBy);

            return pagedData;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<FinancialTransactionOutputDto>> GetByIdAsync(Guid id)
        {
            var financialTransactionResult = await GetAccessibleFinancialTransactionAsync(id);

            if (financialTransactionResult.IsError)
            {
                return financialTransactionResult.Errors;
            }

            var financialTransaction = financialTransactionResult.Value;

            _logger.LogInformation(
                "Financial transaction with ID {FinancialTransactionId} retrieved.",
                id);

            return financialTransaction.ToOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<FinancialTransactionOutputDto>> CreateAsync(
            FinancialTransactionCreateInputDto input)
        {
            var category = await _categoryRepository.GetByIdAsync(input.CategoryId);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", input.CategoryId);
                return Errors.Category.NotFound(input.CategoryId);
            }

            if (category.AppUserId != _currentUser.UserId)
            {
                return Errors.Category.AccessDenied(category.Id);
            }

            var financialTransaction = new FinancialTransaction
            {
                Comment = input.Comment.Trim(),
                Amount = input.Amount,
                Type = input.Type,
                CategoryId = input.CategoryId,
                AppUserId = _currentUser.UserId,
            };

            await _financialTransactionRepository.AddAsync(financialTransaction);

            _logger.LogInformation(
                LoggingEvents.Category.Created,
                "Created new financial transaction {transactionId} for user {UserId}",
                financialTransaction.Id,
                financialTransaction.AppUserId);

            return financialTransaction.ToOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<FinancialTransactionOutputDto>> UpdateAsync(
            Guid id, FinancialTransactionUpdateInputDto input)
        {
            var financialTransactionResult = await GetAccessibleFinancialTransactionAsync(id);
            if (financialTransactionResult.IsError)
            {
                return financialTransactionResult.Errors;
            }

            var financialTransaction = financialTransactionResult.Value;

            var category = await _categoryRepository.GetByIdAsync(input.CategoryId);
            if (category == null)
            {
                _logger.LogWarning(
                    LoggingEvents.Category.NotFound,
                    "Category {CategoryId} not found",
                    input.CategoryId);

                return Errors.Category.NotFound(input.CategoryId);
            }

            if (category.AppUserId != _currentUser.UserId)
            {
                return Errors.Category.AccessDenied(category.Id);
            }

            financialTransaction.CategoryId = input.CategoryId;
            financialTransaction.Amount = input.Amount;
            financialTransaction.Type = input.Type;
            financialTransaction.Comment = input.Comment.Trim();

            await _financialTransactionRepository.UpdateAsync(financialTransaction);

            _logger.LogInformation(
                LoggingEvents.FinancialTransaction.Updated,
                "Financial transaction with ID {FinancialTransactionId} updated.",
                id);

            return financialTransaction.ToOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id)
        {
            var financialTransactionResult = await GetAccessibleFinancialTransactionAsync(id);

            if (financialTransactionResult.IsError)
            {
                return financialTransactionResult.Errors;
            }

            var financialTransaction = financialTransactionResult.Value;

            await _financialTransactionRepository.DeleteAsync(financialTransaction);

            _logger.LogInformation(
                LoggingEvents.FinancialTransaction.Deleted,
                "Financial transaction with ID {FinancialTransactionId} deleted.",
                id);

            return Result.Deleted;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<PagedItems<GroupedReportOutputDto>>> GetReportAsync(ReportQuery query)
        {
            if (!_strategies.TryGetValue(query.Key, out var strategy))
            {
                _logger.LogWarning(
                    LoggingEvents.FinancialTransaction.NotSupportedStrategyGrouping,
                    "Unsupported grouping strategy key has passed");

                return Errors.FT.UnsupportedStrategy(query.Key);
            }

            var spec = new FinancialTransactionReportSpecification(query, _currentUser);
            var grouped = await strategy.GetGroupedAsync(spec, query);

            var count = await _financialTransactionRepository.CountAsync(spec);
            var pagination = new Pagination(query.Page, query.Size, count);

            var pagedData = new PagedItems<GroupedReportOutputDto>
            {
                Items = grouped,
                Pagination = pagination,
            };

            _logger.LogInformation(
                "Returning {Count} financial transactions. Strategy Key = {StrategyKey}, Page={PageNumber}, Size={PageSize}",
                pagedData.Items.Count,
                query.Key,
                pagedData.Pagination.PageNumber,
                pagedData.Pagination.PageSize);

            return pagedData;
        }

        private async Task<ErrorOr<FinancialTransaction>> GetAccessibleFinancialTransactionAsync(Guid id)
        {
            var financialTransaction = await _financialTransactionRepository.GetByIdAsync(id);
            if (financialTransaction == null)
            {
                _logger.LogWarning(
                    LoggingEvents.FinancialTransaction.NotFound,
                    "Financial transaction {FinancialTransactionId} not found",
                    id);
                return Errors.FT.NotFound(id);
            }

            if (financialTransaction.AppUserId != _currentUser.UserId)
            {
                return Errors.FT.AccessDenied(financialTransaction.Id);
            }

            return financialTransaction;
        }
    }
}

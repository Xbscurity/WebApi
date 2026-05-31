using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Providers.CurrentUser;
using api.Providers.Time;
using api.QueryObjects;
using api.Services.Authorization;
using api.Services.Shared;
using api.Services.User;
using api.Specifications;
using ErrorOr;
using System.Collections.Frozen;

namespace api.Services.FinancialTransactions
{
    /// <summary>
    /// Provides application services for managing financial transactions
    /// and generating transaction reports.
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
        private readonly ITimeProvider _timeProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IUserService _userService;
        private readonly ICategoryAccessService _categoryAccessService;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IFinancialTransactionAccessService _financialTransactionAccessService;
        private readonly IFinancialTransactionRepository _financialTransactionRepository;
        private readonly Dictionary<GroupingReportStrategyKey, IGroupingReportStrategy> _strategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger used for diagnostic and audit logging.
        /// </param>
        /// <param name="timeProvider">
        /// The provider used to access the current UTC time.
        /// </param>
        /// <param name="currentUser">
        /// The current authenticated user context.
        /// </param>
        /// <param name="userService">
        /// The service used for user-related operations.
        /// </param>
        /// <param name="сategoryAccessService">
        /// The service used to validate category access permissions.
        /// </param>
        /// <param name="categoryRepository">
        /// The repository used to access category data.
        /// </param>
        /// <param name="financialTransactionAccessService">
        /// The service used to validate financial transaction access permissions.
        /// </param>
        /// <param name="financialTransactionsRepository">
        /// The repository used to manage financial transaction persistence.
        /// </param>
        /// <param name="strategies">
        /// The available grouping strategies for report generation.
        /// </param>
        public FinancialTransactionService(
            ILogger<FinancialTransactionService> logger,
            ITimeProvider timeProvider,
            ICurrentUser currentUser,
            IUserService userService,
            ICategoryAccessService сategoryAccessService,
            IRepository<Category> categoryRepository,
            IFinancialTransactionAccessService financialTransactionAccessService,
            IFinancialTransactionRepository financialTransactionsRepository,
            IEnumerable<IGroupingReportStrategy> strategies)
        {
            _logger = logger;
            _timeProvider = timeProvider;
            _currentUser = currentUser;
            _userService = userService;
            _categoryAccessService = сategoryAccessService;
            _categoryRepository = categoryRepository;
            _financialTransactionAccessService = financialTransactionAccessService;
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
            var financialTransaction = await _financialTransactionRepository.GetByIdAsync(id);
            if (financialTransaction == null)
            {
                _logger.LogWarning(
                    LoggingEvents.FinancialTransaction.NotFound, "Financial transaction {FinancialTransactionId} not found", id);

                return Errors.FT.NotFound(id);
            }

            var financialTransactionAccessCheck = await _financialTransactionAccessService.CanAccessCheckAsync(financialTransaction);
            if (financialTransactionAccessCheck.IsError)
            {
                return financialTransactionAccessCheck.Errors;
            }

            _logger.LogInformation(
                "Financial transaction with ID {FinancialTransactionId} retrieved.",
                id);

            return financialTransaction.ToOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<FinancialTransactionOutputDto>> CreateAsync(
            FinancialTransactionCreateInputDto financialTransactionDto)
        {
            var category = await _categoryRepository.GetByIdAsync(financialTransactionDto.CategoryId);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", financialTransactionDto.CategoryId);
                return Errors.Category.NotFound(financialTransactionDto.CategoryId);
            }

            var categoryAccess = await _categoryAccessService.CanAccessCheckAsync(category);
            if (categoryAccess.IsError)
            {
                return categoryAccess.Errors;
            }

            string finalUserId;
            if (_currentUser.IsAdmin && !string.IsNullOrWhiteSpace(financialTransactionDto.TargetUserId))
            {
                var isExistingUser = await _userService.AnyAsync(financialTransactionDto.TargetUserId);
                if (!isExistingUser)
                {
                    _logger.LogWarning(LoggingEvents.User.NotFound, "Requested User id not found");
                    return Errors.User.NotFound(financialTransactionDto.TargetUserId);
                }

                finalUserId = financialTransactionDto.TargetUserId;
            }
            else
            {
                finalUserId = _currentUser.UserId;
            }

            var transaction = new FinancialTransaction
            {
                Comment = financialTransactionDto.Comment.Trim(),
                Amount = financialTransactionDto.Amount,
                Type = financialTransactionDto.Type,
                CategoryId = financialTransactionDto.CategoryId,
                AppUserId = finalUserId,
                CreatedAt = _timeProvider.UtcNow,
            };

            await _financialTransactionRepository.AddAsync(transaction);

            _logger.LogInformation(
                LoggingEvents.Category.Created,
                "Created new financial transaction {transactionId} for user {UserId}",
                transaction.Id,
                transaction.AppUserId);

            return transaction.ToOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<FinancialTransactionOutputDto>> UpdateAsync(
            Guid id, FinancialTransactionUpdateInputDto transactionDto)
        {
            var financialTransaction = await _financialTransactionRepository.GetByIdAsync(id);
            if (financialTransaction == null)
            {
                _logger.LogWarning(
                    LoggingEvents.FinancialTransaction.NotFound, "Financial transaction {FinancialTransactionId} not found", id);

                return Errors.FT.NotFound(id);
            }

            var financialTransactionAccessCheck = await _financialTransactionAccessService.CanAccessCheckAsync(financialTransaction);
            if (financialTransactionAccessCheck.IsError)
            {
                return financialTransactionAccessCheck.Errors;
            }

            var category = await _categoryRepository.GetByIdAsync(transactionDto.CategoryId);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", transactionDto.CategoryId);
                return Errors.Category.NotFound(transactionDto.CategoryId);
            }

            var categoryAccess = await _categoryAccessService.CanAccessCheckAsync(category);
            if (categoryAccess.IsError)
            {
                return categoryAccess.Errors;
            }

            financialTransaction.CategoryId = transactionDto.CategoryId;
            financialTransaction.Amount = transactionDto.Amount;
            financialTransaction.Type = transactionDto.Type;
            financialTransaction.Comment = transactionDto.Comment.Trim();

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
            var financialTransaction = await _financialTransactionRepository.GetByIdAsync(id);
            if (financialTransaction == null)
            {
                _logger.LogWarning(
                    LoggingEvents.FinancialTransaction.NotFound, "Financial transaction {FinancialTransactionId} not found", id);

                return Errors.FT.NotFound(id);
            }

            var financialTransactionAccessCheck = await _financialTransactionAccessService.CanAccessCheckAsync(financialTransaction);
            if (financialTransactionAccessCheck.IsError)
            {
                return financialTransactionAccessCheck.Errors;
            }

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
    }
}

using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Queries;
using api.Services.Authorization;
using api.Services.Shared;
using api.Services.User;
using api.Specifications;
using ErrorOr;
using System.Collections.Frozen;

namespace api.Services.FinancialTransactions
{
    /// <summary>
    /// Default implementation of <see cref="IAdminFinancialTransactionService"/>.
    /// </summary>
    public class AdminFinancialTransactionService : IAdminFinancialTransactionService
    {
        private static readonly FrozenSet<string> ValidFields = new[]
        {
            "id",
            "category",
            "amount",
            "date",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<FinancialTransactionService> _logger;
        private readonly IUserService _userService;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IFinancialTransactionRepository _financialTransactionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminFinancialTransactionService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger used for diagnostic and audit logging.
        /// </param>
        /// <param name="userService">
        /// The service used for user-related operations.
        /// </param>
        /// <param name="categoryRepository">
        /// The repository used to access category data.
        /// </param>
        /// <param name="financialTransactionsRepository">
        /// The repository used to manage financial transaction persistence.
        /// </param>
        public AdminFinancialTransactionService(
            ILogger<FinancialTransactionService> logger,
            IUserService userService,
            IRepository<Category> categoryRepository,
            IFinancialTransactionRepository financialTransactionsRepository)
        {
            _logger = logger;
            _userService = userService;
            _categoryRepository = categoryRepository;
            _financialTransactionRepository = financialTransactionsRepository;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<PagedItems<AdminFinancialTransactionOutputDto>>> GetAllAsync(
           AdminEntityQuery query)
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

            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);
            var financialTransactions = await _financialTransactionRepository.ListAsync(spec);

            var count = await _financialTransactionRepository.CountAsync(spec);

            var pagination = new Pagination(query.Page, query.Size, count);

            var pagedData = new PagedItems<AdminFinancialTransactionOutputDto>
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
        public async Task<ErrorOr<AdminFinancialTransactionOutputDto>> GetByIdAsync(Guid id)
        {
            var financialTransaction = await _financialTransactionRepository.GetByIdAsync(id);
            if (financialTransaction == null)
            {
                _logger.LogWarning(LoggingEvents.FinancialTransaction.NotFound, "Financial transaction {FinancialTransctionId} not found", id);
                return Errors.FT.NotFound(id);
            }

            _logger.LogInformation(
                "Financial transaction with ID {FinancialTransactionId} retrieved.",
                id);

            return financialTransaction.ToAdminOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<AdminFinancialTransactionOutputDto>> CreateAsync(
            AdminFinancialTransactionCreateInputDto input)
        {
            var category = await _categoryRepository.GetByIdAsync(input.CategoryId);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", input.CategoryId);
                return Errors.Category.NotFound(input.CategoryId);
            }

            var targetUserId = input.AppUserId;

            if (!await _userService.AnyAsync(targetUserId))
            {
                _logger.LogWarning(LoggingEvents.User.NotFound, "Requested User id not found");
                return Errors.User.NotFound(targetUserId);
            }

            if (category.AppUserId != targetUserId)
            {
                return Errors.Category.AccessDenied(input.CategoryId);
            }

            var transaction = new FinancialTransaction
            {
                Comment = input.Comment.Trim(),
                Amount = input.Amount,
                Type = input.Type,
                CategoryId = input.CategoryId,
                AppUserId = targetUserId,
            };

            await _financialTransactionRepository.AddAsync(transaction);

            _logger.LogInformation(
                LoggingEvents.Category.Created,
                "Created new financial transaction {FinancialTransactionId} for user {UserId}",
                transaction.Id,
                transaction.AppUserId);

            return transaction.ToAdminOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<AdminFinancialTransactionOutputDto>> UpdateAsync(
            Guid id, FinancialTransactionUpdateInputDto input)
        {
            var financialTransaction = await _financialTransactionRepository.GetByIdAsync(id);
            if (financialTransaction == null)
            {
                _logger.LogWarning(LoggingEvents.FinancialTransaction.NotFound, "Financial transaction {FinancialTransactionId} not found", id);
                return Errors.FT.NotFound(id);
            }

            var category = await _categoryRepository.GetByIdAsync(input.CategoryId);
            if (category == null)
            {
                _logger.LogWarning(
                    LoggingEvents.Category.NotFound,
                    "Category {CategoryId} not found",
                    input.CategoryId);

                return Errors.Category.NotFound(input.CategoryId);
            }

            if (category.AppUserId != financialTransaction.AppUserId)
            {
                return Errors.Category.AccessDenied(input.CategoryId);
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

            return financialTransaction.ToAdminOutputDto();
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

            await _financialTransactionRepository.DeleteAsync(financialTransaction);

            _logger.LogInformation(
                LoggingEvents.FinancialTransaction.Deleted,
                "Financial transaction with ID {FinancialTransactionId} deleted.",
                id);

            return Result.Deleted;
        }
    }
}

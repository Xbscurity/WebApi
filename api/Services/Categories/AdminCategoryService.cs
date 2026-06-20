using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Providers.CurrentUser;
using api.Queries;
using api.Services.Authorization;
using api.Services.Shared;
using api.Services.User;
using api.Specifications;
using ErrorOr;
using System.Collections.Frozen;

namespace api.Services.Categories
{
    /// <summary>
    /// Default implementation of <see cref="IAdminCategoryService"/>.
    /// </summary>
    public class AdminCategoryService : IAdminCategoryService
    {
        private static readonly FrozenSet<string> ValidFields = new[]
        {
            "id",
            "name",
            "isactive",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<CategoryService> _logger;
        private readonly IUserService _userService;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<FinancialTransaction> _financialTransactionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminCategoryService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger used for diagnostic and audit logging.
        /// </param>
        /// <param name="userService">
        /// The service used for user-related operations.
        /// </param>
        /// <param name="categoriesRepository">
        /// The repository used to manage category persistence and retrieval.
        /// </param>
        /// <param name="financialTransactionRepository">
        /// The repository used to access financial transaction data.
        /// </param>
        public AdminCategoryService(
            ILogger<CategoryService> logger,
            IUserService userService,
            IRepository<Category> categoriesRepository,
            IRepository<FinancialTransaction> financialTransactionRepository)
        {
            _logger = logger;
            _userService = userService;
            _categoryRepository = categoriesRepository;
            _financialTransactionRepository = financialTransactionRepository;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<PagedItems<AdminCategoryOutputDto>>> GetAllAsync(
                    AdminEntityQuery query)
        {
            if (!ValidFields.Contains(query.SortBy))
            {
                var allowed = string.Join(", ", ValidFields);

                _logger.LogWarning(
                    LoggingEvents.Category.SortInvalid,
                    "SortBy '{Field}' is invalid. Allowed fields: {AllowedFields}",
                    query.SortBy,
                    allowed);

                return Errors.Category.InvalidSortBy(query.SortBy, ValidFields);
            }

            var spec = new AdminCategorySortedPagedSpecification(query);
            var categories = await _categoryRepository.ListAsync(spec);
            var count = await _categoryRepository.CountAsync(spec);

            var pagination = new Pagination(query.Page, query.Size, count);
            var pagedData = new PagedItems<AdminCategoryOutputDto>
            {
                Items = categories,
                Pagination = pagination,
            };

            _logger.LogInformation(
                "Returning {Count} categories. Page={PageNumber}, Size={PageSize}, SortBy={SortBy}, UserId = {UserId}",
                pagedData.Items.Count,
                pagedData.Pagination.PageNumber,
                pagedData.Pagination.PageSize,
                query.SortBy,
                query.UserId);

            return pagedData;
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<AdminCategoryOutputDto>> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            return category.ToAdminOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<AdminCategoryOutputDto>> CreateAsync(
            AdminCategoryCreateInputDto input)
        {
            var targetUserId = input.AppUserId;
            if (!await _userService.AnyAsync(targetUserId))
            {
                _logger.LogWarning(LoggingEvents.User.NotFound, "Requested User id not found");
                return Errors.User.NotFound(targetUserId);
            }

            var category = new Category
            {
                Name = input.Name.Trim(),
                AppUserId = targetUserId,
            };

            await _categoryRepository.AddAsync(category);

            _logger.LogInformation(
                LoggingEvents.Category.Created,
                "Created new category {categoryId} for {AppUserId}",
                category.Id,
                input.AppUserId);

            return category.ToAdminOutputDto();
        }

        /// <inheritdoc />
        public async Task<ErrorOr<AdminCategoryOutputDto>> UpdateAsync(
            Guid id, CategoryUpdateInputDto input)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            category.Name = input.Name.Trim();

            await _categoryRepository.UpdateAsync(category);

            _logger.LogInformation(
                LoggingEvents.Category.Updated,
                "Category {CategoryId} updated.",
                category.Id);

            return category.ToAdminOutputDto();
        }

        /// <inheritdoc />
        public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            var spec = new FinancialTransactionByCategoryIdSpecification(id);
            if (await _financialTransactionRepository.AnyAsync(spec))
            {
                _logger.LogWarning(
                LoggingEvents.Category.DeleteRestricted,
                "Delete blocked: Category {CategoryId} has existing related entities.",
                category.Id);

                return Errors.Category.DeleteRestricted();
            }

            await _categoryRepository.DeleteAsync(category);

            _logger.LogInformation(
                LoggingEvents.Category.Deleted,
                "Category {CategoryId} deleted.",
                category.Id);

            return Result.Deleted;
        }

        /// <inheritdoc />
        public async Task<ErrorOr<ToggleActiveOutputDto>> SetActiveAsync(Guid id, bool isActive)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            category.IsActive = isActive;

            await _categoryRepository.UpdateAsync(category);

            _logger.LogInformation(
                LoggingEvents.Category.Toggled,
                "Category {CategoryId} active status successfully toggled.",
                category.Id);
            var outputDto = new ToggleActiveOutputDto
            {
                ToggleActive = category.IsActive,
            };
            return outputDto;
        }
    }
}

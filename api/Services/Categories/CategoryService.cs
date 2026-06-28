using api.Constants;
using api.Data;
using api.Dtos.Category;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Providers.CurrentUser;
using api.Queries;
using api.Services.Shared;
using api.Services.User;
using api.Specifications;
using Ardalis.Specification;
using ErrorOr;
using System.Collections.Frozen;

namespace api.Services.Categories
{
    /// <summary>
    /// Default implementation of <see cref="ICategoryService"/>.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private static readonly FrozenSet<string> ValidFields = new[]
        {
            "id",
            "name",
            "isactive",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<CategoryService> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly IUserService _userService;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<FinancialTransaction> _financialTransactionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger used for diagnostic and audit logging.
        /// </param>
        /// <param name="currentUser">
        /// The current authenticated user context.
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
        public CategoryService(
            ILogger<CategoryService> logger,
            ICurrentUser currentUser,
            IUserService userService,
            IRepository<Category> categoriesRepository,
            IRepository<FinancialTransaction> financialTransactionRepository)
        {
            _logger = logger;
            _currentUser = currentUser;
            _userService = userService;
            _categoryRepository = categoriesRepository;
            _financialTransactionRepository = financialTransactionRepository;
        }

        /// <inheritdoc />
        public async Task<ErrorOr<PagedItems<CategoryOutputDto>>> GetAllAsync(
            EntityQuery query)
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

            var spec = new CategorySortedPagedSpecification(query, _currentUser);
            var categories = await _categoryRepository.ListAsync(spec);
            var count = await _categoryRepository.CountAsync(spec);

            var pagination = new Pagination(query.Page, query.Size, count);
            var pagedData = new PagedItems<CategoryOutputDto>
            {
                Items = categories,
                Pagination = pagination,
            };

            _logger.LogInformation(
                "Returning {Count} categories. Page={PageNumber}, Size={PageSize}, SortBy={SortBy}",
                pagedData.Items.Count,
                pagedData.Pagination.PageNumber,
                pagedData.Pagination.PageSize,
                query.SortBy);

            return pagedData;
        }

        /// <inheritdoc />
        public async Task<ErrorOr<CategoryOutputDto>> GetByIdAsync(Guid id)
        {
            var categoryResult = await GetAccessibleCategoryAsync(id);
            if (categoryResult.IsError)
            {
                return categoryResult.Errors;
            }

            return categoryResult.Value.ToOutputDto();
        }

        /// <inheritdoc />
        public async Task<ErrorOr<CategoryOutputDto>> CreateAsync(
            CategoryCreateInputDto input)
        {
            var category = new Category
            {
                Name = input.Name.Trim(),
                AppUserId = _currentUser.UserId,
            };

            await _categoryRepository.AddAsync(category);

            _logger.LogInformation(
                LoggingEvents.Category.Created,
                "Created new category {categoryId}",
                category.Id);

            return category.ToOutputDto();
        }

        /// <inheritdoc />
        public async Task<ErrorOr<CategoryOutputDto>> UpdateAsync(
            Guid id, CategoryUpdateInputDto input)
        {
            var categoryResult = await GetAccessibleCategoryAsync(id);
            if (categoryResult.IsError)
            {
                return categoryResult.Errors;
            }

            var category = categoryResult.Value;
            category.Name = input.Name.Trim();

            await _categoryRepository.UpdateAsync(category);

            _logger.LogInformation(
                LoggingEvents.Category.Updated,
                "Category {CategoryId} updated.",
                category.Id);
            return category.ToOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<ToggleActiveOutputDto>> SetActiveAsync(Guid id, bool isActive)
        {
            var categoryResult = await GetAccessibleCategoryAsync(id);

            if (categoryResult.IsError)
            {
                return categoryResult.Errors;
            }

            var category = categoryResult.Value;

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

        /// <inheritdoc/>
        public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id)
        {
            var categoryResult = await GetAccessibleCategoryAsync(id);

            if (categoryResult.IsError)
            {
                return categoryResult.Errors;
            }

            var category = categoryResult.Value;

            var spec = new FinancialTransactionByCategoryIdSpecification(id);
            if (await _financialTransactionRepository.AnyAsync(spec))
            {
                _logger.LogWarning(
                LoggingEvents.Category.DeleteRestricted,
                "Delete blocked: Category {CategoryId} has existing related entities.",
                category.Id);

                return Errors.Category.DeleteRestricted(id);
            }

            await _categoryRepository.DeleteAsync(category);

            _logger.LogInformation(
                LoggingEvents.Category.Deleted,
                "Category {CategoryId} deleted.",
                category.Id);

            return Result.Deleted;
        }

        /// <inheritdoc />
        public async Task<ErrorOr<Success>> CreateInitialCategoriesForUserAsync(string userId)
        {
            var templates = DataSeeder.DefaultCategoryTemplates;

            var userCategories = templates.Select(template => new Category
            {
                Name = template.Name,
                AppUserId = userId,
                IsActive = true,
            }).ToList();

            await _categoryRepository.AddRangeAsync(userCategories);

            return Result.Success;
        }

        private async Task<ErrorOr<Category>> GetAccessibleCategoryAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            if (category.AppUserId != _currentUser.UserId)
            {
                return Errors.Category.AccessDenied(category.Id);
            }

            return category;
        }
    }
}
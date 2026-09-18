using api.Constants;
using api.Data;
using api.Dtos.Category;
using api.Extensions;
using api.Models;
using api.Providers.CurrentUser;
using api.Queries;
using api.Repositories;
using api.Services.Shared;
using api.Specifications.Categories;
using api.Specifications.FinancialTransactions;
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
            "name",
            "isactive",
            "createdat",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<CategoryService> _logger;
        private readonly ICurrentUser _currentUser;
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
        /// <param name="categoriesRepository">
        /// The repository used to manage category persistence and retrieval.
        /// </param>
        /// <param name="financialTransactionRepository">
        /// The repository used to access financial transaction data.
        /// </param>
        public CategoryService(
            ILogger<CategoryService> logger,
            ICurrentUser currentUser,
            IRepository<Category> categoriesRepository,
            IRepository<FinancialTransaction> financialTransactionRepository)
        {
            _logger = logger;
            _currentUser = currentUser;
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

                _logger.LogInformation(
                    LoggingEvents.Category.SortInvalid,
                    "SortBy '{Field}' is invalid. Allowed fields: {AllowedFields}",
                    query.SortBy,
                    allowed);

                return Errors.Category.InvalidSortBy(query.SortBy, ValidFields);
            }

            var spec = new CategorySortedPagedSpecification(query, _currentUser.UserId);
            var categories = await _categoryRepository.ListAsync(spec);
            var count = await _categoryRepository.CountAsync(spec);

            var pagination = new Pagination(query.Page, query.Size, count);
            var pagedData = new PagedItems<CategoryOutputDto>
            {
                Items = categories,
                Pagination = pagination,
            };

            _logger.LogDebug(
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
            var category = await _categoryRepository
                .FirstOrDefaultAsync(new CategoryByIdSpecification(id, _currentUser.UserId));

            if (category == null)
            {
                _logger.LogInformation(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            _logger.LogDebug(
                "Category with ID {CategoryId} retrieved.",
                id);
            return category;
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

            var spec = new HasCategoryWithNameSpecification(_currentUser.UserId, input.Name);
            if (await _categoryRepository.AnyAsync(spec))
            {
                return Errors.Category.NameAlreadyExists(input.Name);
            }

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

            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation(
                LoggingEvents.Category.Updated,
                "Category {CategoryId} updated.",
                category.Id);
            return category.ToOutputDto();
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<SetActiveOutputDto>> SetActiveAsync(Guid id, SetActiveInputDto input)
        {
            var categoryResult = await GetAccessibleCategoryAsync(id);

            if (categoryResult.IsError)
            {
                return categoryResult.Errors;
            }

            var category = categoryResult.Value;

            category.IsActive = input.IsActive;

            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation(
                LoggingEvents.Category.SetActive,
                "Category {CategoryId} active status successfully set to {IsActive}.",
                category.Id,
                input.IsActive);

            var outputDto = new SetActiveOutputDto
            {
                IsActive = category.IsActive,
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

            var spec = new HasFinancialTransactionsByCategoryIdSpecification(id);
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
        public async Task CreateInitialCategoriesForUserAsync(string userId)
        {
            var templates = DataSeeder.DefaultCategoryTemplates;

            var userCategories = templates.Select(template => new Category
            {
                Name = template.Name,
                AppUserId = userId,
                IsActive = true,
            }).ToList();

            await _categoryRepository.AddRangeAsync(userCategories);
        }

        private async Task<ErrorOr<Category>> GetAccessibleCategoryAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogInformation(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            if (category.AppUserId != _currentUser.UserId)
            {
                _logger.LogWarning(LoggingEvents.Category.AccessDenied, "Access denied to Category {CategoryId}", id);
                return Errors.Category.NotFound(category.Id);
            }

            return category;
        }
    }
}
using api.Constants;
using api.Data;
using api.Dtos.Category;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Providers.CurrentUser;
using api.QueryObjects;
using api.Services.Authorization;
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
        private readonly ICategoryAccessService _categoryAccessService;
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
        /// <param name="categoryAccessService">
        /// The service used to validate category access permissions.
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
            ICategoryAccessService categoryAccessService,
            IRepository<Category> categoriesRepository,
            IRepository<FinancialTransaction> financialTransactionRepository)
        {
            _logger = logger;
            _currentUser = currentUser;
            _userService = userService;
            _categoryAccessService = categoryAccessService;
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
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            var categoryAccess = await _categoryAccessService.CanAccessCheckAsync(category);
            if (categoryAccess.IsError)
            {
                return categoryAccess.Errors;
            }

            _logger.LogDebug(
                "Category {CategoryId} retrieved.",
                id);

            return category.ToOutputDto();
        }

        /// <inheritdoc />
        public async Task<ErrorOr<CategoryOutputDto>> CreateAsync(
            CategoryCreateInputDto categoryDto)
        {
            string finalUserId;
            if (_currentUser.IsAdmin && !string.IsNullOrWhiteSpace(categoryDto.TargetUserId))
            {
                var isExistingUser = await _userService.AnyAsync(categoryDto.TargetUserId);
                if (!isExistingUser)
                {
                    _logger.LogWarning(LoggingEvents.User.NotFound, "Requested User id not found");
                    return Errors.User.NotFound(categoryDto.TargetUserId);
                }

                finalUserId = categoryDto.TargetUserId;
            }
            else
            {
                finalUserId = _currentUser.UserId;
            }

            Category category = new Category
            {
                Name = categoryDto.Name.Trim(),
                AppUserId = finalUserId,
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
            Guid id, CategoryUpdateInputDto categoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            var categoryAccess = await _categoryAccessService.CanAccessCheckAsync(category);
            if (categoryAccess.IsError)
            {
                return categoryAccess.Errors;
            }

            category!.Name = categoryDto.Name!.Trim();

            await _categoryRepository.UpdateAsync(category);

            _logger.LogInformation(
                LoggingEvents.Category.Updated,
                "Category {CategoryId} updated.",
                category.Id);

            return category.ToOutputDto();
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

            var categoryAccess = await _categoryAccessService.CanAccessCheckAsync(category);
            if (categoryAccess.IsError)
            {
                return categoryAccess.Errors;
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

        /// <inheritdoc />
        public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning(LoggingEvents.Category.NotFound, "Category {CategoryId} not found", id);
                return Errors.Category.NotFound(id);
            }

            var categoryAccess = await _categoryAccessService.CanAccessCheckAsync(category);
            if (categoryAccess.IsError)
            {
                return categoryAccess.Errors;
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
    }
}
using api.Constants;
using api.Data;
using api.Dtos.Category;
using api.Extensions;
using api.Models;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Responses;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Categories
{
    /// <summary>
    /// Implements category-related business logic, including user-specific
    /// and admin-level category management operations.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        /// <param name="categoriesRepository">The repository for category persistence and retrieval.</param>
        /// /// <param name="logger">The logger for recording category events.</param>
        public CategoryService(ICategoryRepository categoriesRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoriesRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<PagedData<BaseCategoryOutputDto>> GetAllForUserAsync(
            string userId, PaginationQueryObject queryObject, bool includeInactive)
        {
            var query = _categoryRepository.GetQueryable(includeInactive)
              .Where(c => c.AppUserId == userId); // Categories for current user
            var result = await query.ApplySorting(queryObject)
                .Select(c => c.ToOutputDto())
                .ToPagedQueryAsync(queryObject);

            return new PagedData<BaseCategoryOutputDto>
            {
                Data = await result.Query.ToListAsync(),
                Pagination = result.Pagination,
            };
        }

        /// <inheritdoc />
        public async Task<PagedData<BaseCategoryOutputDto>> GetAllForAdminAsync(PaginationQueryObject queryObject, string? userId)
        {
            var query = _categoryRepository.GetQueryable();

            if (userId != null)
            {
                query = query.Where(c => c.AppUserId == userId);
            }

            var pagedQuery = await query.ApplySorting(queryObject).ToPagedQueryAsync(queryObject);

            return new PagedData<BaseCategoryOutputDto>
            {
                Data = await pagedQuery.Query.Select(c => c.ToOutputDto()).ToListAsync(),
                Pagination = pagedQuery.Pagination,
            };
        }

        /// <inheritdoc />
        public async Task<string> ToggleActiveAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id, includeInactive: true);

            category!.IsActive = !category.IsActive;

            await _categoryRepository.UpdateAsync(category);

            _logger.LogDebug(
                LoggingEvents.Categories.Common.Toggled,
                "Category with ID {CategoryId} active status successfully toggled.",
                id);
            return category.IsActive ? "Category activated" : "Category deactivated";
        }

        /// <inheritdoc />
        public async Task<BaseCategoryOutputDto?> GetByIdAsync(int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);

            return existingCategory!.ToOutputDto();
        }

        /// <inheritdoc />
        public async Task<BaseCategoryOutputDto> CreateForUserAsync(string userId, BaseCategoryUpdateInputDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name!.Trim(),
                AppUserId = userId,
            };
            await _categoryRepository.CreateAsync(category);

            return category.ToOutputDto();
        }

        /// <inheritdoc />
        public async Task<BaseCategoryOutputDto> CreateForAdminAsync(AdminCategoryCreateInputDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name!.Trim(),
                AppUserId = categoryDto.AppUserId,
            };
            await _categoryRepository.CreateAsync(category);

            return category.ToOutputDto();
        }

        /// <inheritdoc />
        public async Task<BaseCategoryOutputDto?> UpdateAsync(int id, BaseCategoryUpdateInputDto categoryDto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);

            existingCategory!.Name = categoryDto.Name!.Trim();

            await _categoryRepository.UpdateAsync(existingCategory);

            return existingCategory.ToOutputDto();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);

            await _categoryRepository.DeleteAsync(existingCategory!);

            return true;
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

            await _categoryRepository.CreateRangeAsync(userCategories);
        }
    }
}
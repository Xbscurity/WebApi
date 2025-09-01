using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.Models;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Responses;
using api.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoriesRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoriesRepository;
            _logger = logger;
        }

        public async Task<PagedData<BaseCategoryOutputDto>> GetAllForUserAsync(
            CurrentUser user, PaginationQueryObject queryObject, bool includeInactive)
        {
            var query = _categoryRepository.GetQueryable()
        .Where(c =>
            // User's own categories (include inactive if requested)
            c.AppUserId == user.UserId && (includeInactive || c.IsActive)
            ||
            // All active global categories
            c.AppUserId == null && c.IsActive);

            var result = await query.ApplySorting(queryObject).Select(c => c.ToOutputDto()).ToPagedQueryAsync(queryObject);
            return new PagedData<BaseCategoryOutputDto>
            {
                Data = await result.Query.ToListAsync(),
                Pagination = result.Pagination,
            };
        }

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

        public async Task<bool> ToggleActiveAsync(CurrentUser user, int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogDebug("Category {CategoryId} not found", id);
                return false;
            }

            if (!CanAccessCategory(user, category))
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.NoAccess, "Access denied to category {CategoryId}", id);
                return false;
            }

            category.IsActive = !category.IsActive;
            await _categoryRepository.UpdateAsync(category);

            return true;
        }

        public async Task<BaseCategoryOutputDto?> GetByIdAsync(CurrentUser user, int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                _logger.LogDebug("Category {CategoryId} not found", id);
                return null;
            }

            if (!CanAccessCategory(user, existingCategory, allowGlobal: true))
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.NoAccess, "Access denied to category {CategoryId}", id);
                return null;
            }

            return existingCategory.ToOutputDto();
        }

        public async Task<BaseCategoryOutputDto> CreateForUserAsync(CurrentUser user, BaseCategoryInputDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name!.Trim(),
                AppUserId = user.UserId,
            };
            await _categoryRepository.CreateAsync(category);
            return category.ToOutputDto();
        }

        public async Task<BaseCategoryOutputDto> CreateForAdminAsync(AdminCategoryInputDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name!.Trim(),
                AppUserId = categoryDto.AppUserId,
            };
            await _categoryRepository.CreateAsync(category);
            return category.ToOutputDto();
        }

        public async Task<BaseCategoryOutputDto?> UpdateAsync(CurrentUser user, int id, BaseCategoryInputDto categoryDto)
        {

            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory is null)
            {
                _logger.LogDebug("Category {CategoryId} not found", id);
                return null;
            }

            if (!CanAccessCategory(user, existingCategory))
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.NoAccess, "Access denied to category {CategoryId}", id);
                return null;
            }

            existingCategory.Name = categoryDto.Name!.Trim();
            await _categoryRepository.UpdateAsync(existingCategory);
            return existingCategory.ToOutputDto();
        }

        public async Task<bool> DeleteAsync(CurrentUser user, int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory is null)
            {
                _logger.LogDebug("Category {CategoryId} not found", id);
                return false;
            }

            if (!CanAccessCategory(user, existingCategory))
            {
                _logger.LogWarning(LoggingEvents.Categories.Common.NoAccess, "Access denied to category {CategoryId}", id);
                return false;
            }

            await _categoryRepository.DeleteAsync(existingCategory);
            return true;
        }

        private bool CanAccessCategory(CurrentUser user, Category category, bool allowGlobal = false)
        {
            bool isOwner = category.AppUserId == user.UserId;
            bool includeGlobal = allowGlobal && category.AppUserId == null;

            return user.IsAdmin || isOwner || includeGlobal;
        }
    }
}
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

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        /// <param name="categoriesRepository">The repository for category persistence and retrieval.</param>
        public CategoryService(ICategoryRepository categoriesRepository)
        {
            _categoryRepository = categoriesRepository;
        }

        /// <inheritdoc />
        public async Task<PagedData<BaseCategoryOutputDto>> GetAllForUserAsync(
            string userId, PaginationQueryObject queryObject, bool includeInactive)
        {
            var query = _categoryRepository.GetQueryable()
        .Where(c =>
            // User's own categories (include inactive if requested)
            (c.AppUserId == userId && (includeInactive || c.IsActive))
            ||
            // All active global categories
            (c.AppUserId == null && c.IsActive));

            var result = await query.ApplySorting(queryObject).Select(c => c.ToOutputDto()).ToPagedQueryAsync(queryObject);
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
        public async Task<bool> ToggleActiveAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            category.IsActive = !category.IsActive;
            await _categoryRepository.UpdateAsync(category);

            return true;
        }

        /// <inheritdoc />
        public async Task<BaseCategoryOutputDto?> GetByIdAsync(int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                return null;
            }

            return existingCategory.ToOutputDto();
        }

        /// <inheritdoc />
        public async Task<Category?> GetByIdRawAsync(int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                return null;
            }

            return existingCategory;
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
            if (existingCategory is null)
            {
                return null;
            }

            existingCategory.Name = categoryDto.Name!.Trim();
            await _categoryRepository.UpdateAsync(existingCategory);
            return existingCategory.ToOutputDto();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory is null)
            {
                return false;
            }

            await _categoryRepository.DeleteAsync(existingCategory);
            return true;
        }
    }
}
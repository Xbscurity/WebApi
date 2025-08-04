using api.Constants;
using api.Dtos.Category;
using api.Extensions;
using api.Models;
using api.QueryObjects;
using api.Repositories.Categories;
using api.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoriesRepository)
        {
            _categoryRepository = categoriesRepository;
        }

        public async Task<PagedData<BaseCategoryOutputDto>> GetAllForUserAsync(
            ClaimsPrincipal user, PaginationQueryObject queryObject, bool includeInactive)
        {
            var userId = user.GetUserId();
            var query = _categoryRepository.GetQueryable()
        .Where(c =>
            c.AppUserId == userId && (includeInactive || c.IsActive)
            ||
            c.AppUserId == null && c.IsActive);

            var result = await query.ApplySorting(queryObject).Select(c => c.ToOutputDto()).ToPagedQueryAsync(queryObject);
            return new PagedData<BaseCategoryOutputDto>
            {
                Data = await result.Query.ToListAsync(),
                Pagination = result.Pagination,
            };
        }

        public async Task<PagedData<Models.Category>> GetAllForAdminAsync(PaginationQueryObject queryObject, string? userId)
        {
            var query = _categoryRepository.GetQueryable();
            if (userId != null)
            {
                query = query.Where(c => c.AppUserId == userId);
            }

            var result = await query.ApplySorting(queryObject).IgnoreQueryFilters().ToPagedQueryAsync(queryObject);
            return new PagedData<Models.Category>
            {
                Data = await result.Query.ToListAsync(),
                Pagination = result.Pagination,
            };
        }

        public async Task<bool> ToggleActiveAsync(ClaimsPrincipal user, int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            if (user.IsInRole(Roles.User) && category.AppUserId != user.GetUserId())
            {
                return false;
            }

            category.IsActive = !category.IsActive;
            await _categoryRepository.UpdateAsync(category);

            return true;
        }

        public async Task<BaseCategoryOutputDto?> GetByIdAsync(ClaimsPrincipal user, int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                return null;
            }

            if (!HasAccess(user, existingCategory))
            {
                return null;
            }

            return existingCategory.ToOutputDto();
        }

        public async Task<BaseCategoryOutputDto> CreateForUserAsync(ClaimsPrincipal user, BaseCategoryInputDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name!.Trim(),
                AppUserId = user.GetUserId(),
            };
            await _categoryRepository.CreateAsync(category);
            return category.ToOutputDto();
        }

        public async Task<Category> CreateForAdminAsync(ClaimsPrincipal user, AdminCategoryInputDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name!.Trim(),
                AppUserId = categoryDto.AppUserId,
            };
            await _categoryRepository.CreateAsync(category);
            return category;
        }

        public async Task<BaseCategoryOutputDto?> UpdateAsync(ClaimsPrincipal user, int id, BaseCategoryInputDto categoryDto)
        {

            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory is null)
            {
                return null;
            }

            if (!HasAccess(user, existingCategory))
            {
                return null;
            }

            existingCategory.Name = categoryDto.Name!.Trim();
            await _categoryRepository.UpdateAsync(existingCategory);
            return existingCategory.ToOutputDto();
        }

        public async Task<bool> DeleteAsync(ClaimsPrincipal user, int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory is null)
            {
                return false;
            }

            if (!HasAccess(user, existingCategory))
            {
                return false;
            }

            await _categoryRepository.DeleteAsync(existingCategory);
            return true;
        }

        private bool HasAccess(ClaimsPrincipal user, Category category)
        {
            var isAdmin = user.IsInRole(Roles.Admin);
            return isAdmin || category.AppUserId == user.GetUserId() || category.AppUserId == null;
        }
    }
}
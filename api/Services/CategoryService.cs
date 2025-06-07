using api.Dtos;
using api.Extensions;
using api.Helpers;
using api.Models;
using api.QueryObjects;
using api.Repositories.Interfaces;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoriesRepository)
        {
            _categoryRepository = categoriesRepository;
        }

        public async Task<PagedData<Category>> GetAllAsync(PaginationQueryObject queryObject)
        {
            var query = _categoryRepository.GetQueryable();
            var result = await query.ApplySorting(queryObject).ToPagedQueryAsync(queryObject);
            return new PagedData<Category>
            {
                Data = await result.Query.ToListAsync(),
                Pagination = result.Pagination,
            };
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category> CreateAsync(CategoryInputDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name!.Trim(),
            };
            await _categoryRepository.CreateAsync(category);
            return category;
        }

        public async Task<Category?> UpdateAsync(int id, CategoryInputDto categoryDto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory is null)
            {
                return null;
            }

            existingCategory.Name = categoryDto.Name!.Trim();
            await _categoryRepository.UpdateAsync(existingCategory);
            return existingCategory;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
            {
                return false;
            }

            await _categoryRepository.DeleteAsync(category);
            return true;
        }
    }
}

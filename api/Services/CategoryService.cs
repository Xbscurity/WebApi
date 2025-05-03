using api.Dtos;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

namespace api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoriesRepository)
        {
            _categoryRepository = categoriesRepository;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category> CreateAsync(CategoryDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name!
            };
            await _categoryRepository.CreateAsync(category);
            return category;
        }

        public async Task<Category?> UpdateAsync(int id, CategoryDto categoryDto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory is null)
            {
                return null;
            }
            existingCategory.Name = categoryDto.Name!;
            await _categoryRepository.UpdateAsync(existingCategory);

            return existingCategory;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                await _categoryRepository.DeleteAsync(category);
                return true;
            }
            return false;
        }
    }
}

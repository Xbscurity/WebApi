using api.Dtos;
using api.Models;

namespace api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(CategoryDto category);
        Task<Category?> UpdateAsync(int id, CategoryDto category);
        Task<bool> DeleteAsync(int id);
    }
}
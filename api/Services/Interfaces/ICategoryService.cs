using api.Dtos;
using api.Helpers;
using api.Models;
using api.QueryObjects;

namespace api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedData<Category>> GetAllAsync(PaginationQueryObject queryObject);

        Task<Category?> GetByIdAsync(int id);

        Task<Category> CreateAsync(CategoryInputDto category);

        Task<Category?> UpdateAsync(int id, CategoryInputDto category);

        Task<bool> DeleteAsync(int id);
    }
}
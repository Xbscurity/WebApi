using api.Dtos.Category;
using api.Models;
using api.QueryObjects;
using api.Responses;

namespace api.Services.Categories
{
    public interface ICategoryService
    {
        Task<PagedData<BaseCategoryOutputDto>> GetAllForUserAsync(string userId, PaginationQueryObject queryObject, bool includeInactive);

        Task<bool> ToggleActiveAsync(int id);

        Task<PagedData<BaseCategoryOutputDto>> GetAllForAdminAsync(PaginationQueryObject queryObject, string? userId);

        Task<BaseCategoryOutputDto?> GetByIdAsync(int id);

        Task<Category?> GetByIdRawAsync(int id);

        Task<BaseCategoryOutputDto> CreateForAdminAsync(AdminCategoryInputDto category);

        Task<BaseCategoryOutputDto> CreateForUserAsync(string userId, BaseCategoryInputDto categoryDto);

        Task<BaseCategoryOutputDto?> UpdateAsync(int id, BaseCategoryInputDto category);

        Task<bool> DeleteAsync(int id);

    }
}
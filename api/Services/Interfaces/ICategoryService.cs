using api.Dtos.Category;
using api.Helpers;
using api.Models;
using api.QueryObjects;
using System.Security.Claims;

namespace api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedData<BaseCategoryOutputDto>> GetAllForUserAsync(
            ClaimsPrincipal user, PaginationQueryObject queryObject, bool includeInactive);

        Task<bool> ToggleActiveAsync(ClaimsPrincipal user, int id);

        Task<PagedData<Category>> GetAllForAdminAsync(PaginationQueryObject queryObject, string? userId);

        Task<BaseCategoryOutputDto?> GetByIdAsync(ClaimsPrincipal user, int id);

        Task<Category> CreateForAdminAsync(ClaimsPrincipal user, AdminCategoryInputDto category);

        Task<BaseCategoryOutputDto> CreateForUserAsync(ClaimsPrincipal user, BaseCategoryInputDto categoryDto);

        Task<BaseCategoryOutputDto?> UpdateAsync(ClaimsPrincipal user, int id, BaseCategoryInputDto category);

        Task<bool> DeleteAsync(ClaimsPrincipal user, int id);

    }
}
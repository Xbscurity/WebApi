using api.Dtos.Category;
using api.QueryObjects;
using api.Responses;
using api.Services.Common;

namespace api.Services.Categories
{
    public interface ICategoryService
    {
        Task<PagedData<BaseCategoryOutputDto>> GetAllForUserAsync(
            CurrentUser user, PaginationQueryObject queryObject, bool includeInactive);

        Task<bool> ToggleActiveAsync(CurrentUser user, int id);

        Task<PagedData<BaseCategoryOutputDto>> GetAllForAdminAsync(PaginationQueryObject queryObject, string? userId);

        Task<BaseCategoryOutputDto?> GetByIdAsync(CurrentUser user, int id);

        Task<BaseCategoryOutputDto> CreateForAdminAsync(AdminCategoryInputDto category);

        Task<BaseCategoryOutputDto> CreateForUserAsync(CurrentUser user, BaseCategoryInputDto categoryDto);

        Task<BaseCategoryOutputDto?> UpdateAsync(CurrentUser user, int id, BaseCategoryInputDto category);

        Task<bool> DeleteAsync(CurrentUser user, int id);

    }
}
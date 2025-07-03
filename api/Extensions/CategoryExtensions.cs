using api.Dtos.Category;
using api.Models;

namespace api.Extensions
{
    public static class CategoryExtensions
    {
        public static BaseCategoryOutputDto ToOutputDto(this Category category)
        {
            if (category is null)
            {
                return null;
            }

            return new BaseCategoryOutputDto
            {
                Id = category.Id,
                Name = category.Name,
                AppUserId = category.AppUserId,
                IsActive = category.IsActive,
            };
        }
    }
}

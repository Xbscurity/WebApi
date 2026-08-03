using api.Models;

namespace api.Tests.Unit.Factories
{
    public static class CategoryFactory
    {
        public static Category Create(
            Guid? id = null,
            string userId = "1234",
            string name = "Old Name",
            bool isActive = true,
            DateTimeOffset? createdAt = null,
            DateTimeOffset? updatedAt = null)
        {
            return new Category
            {
                Id = id ?? Guid.NewGuid(),
                AppUserId = userId,
                Name = name,
                IsActive = isActive,
                CreatedAt = createdAt ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = updatedAt ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
        }
    }
}

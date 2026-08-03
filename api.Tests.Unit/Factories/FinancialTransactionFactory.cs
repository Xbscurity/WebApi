using api.Enums;
using api.Models;

namespace api.Tests.Unit.Factories
{
    public static class FinancialTransactionFactory
    {
        public static FinancialTransaction Create(
            Guid? id = null,
            Guid? categoryId = null,
            string userId = "1234",
            decimal amount = 100,
            FinancialTransactionType type = FinancialTransactionType.Income,
            DateTimeOffset? createdAt = null,
            DateTimeOffset? updatedAt = null,
            bool isActive = true,
            string comment = "Apple",
            string categoryName = "Groceries"
            )
        {
            var _categoryId = categoryId ?? Guid.NewGuid();
            return new FinancialTransaction
            {
                Id = id ?? Guid.NewGuid(),
                CategoryId = _categoryId,
                AppUserId = userId,
                Amount = amount,
                Type = type,
                Comment = comment,
                CreatedAt = createdAt ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = updatedAt ?? new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Category = CategoryFactory.Create(id: _categoryId, isActive: isActive, name: categoryName)
            };
        }
    }
}

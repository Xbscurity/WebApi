using api.Enums;
using api.Models;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class AdminFinancialTransactionByIdSpecificationTests
    {
        private static readonly Guid transactionId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid transactionId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid nonExistentId = Guid.Parse("00000000-0000-0000-0000-000000000099");

        private static readonly Guid categoryId1 = Guid.Parse("00000000-0000-0000-0000-000000000101");
        private static readonly Guid categoryId2 = Guid.Parse("00000000-0000-0000-0000-000000000102");

        private static readonly DateTimeOffset createdAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt2 = new(2026, 1, 1, 2, 0, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset updatedAt1 = new(2027, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt2 = new(2027, 1, 1, 2, 0, 0, 0, TimeSpan.Zero);

        private readonly List<FinancialTransaction> _transactions = new()
        {
            FinancialTransactionFactory.Create(
                id: transactionId1,
                categoryId: categoryId1,
                userId: "user-1",
                amount: 150.50m,
                comment: "Apple",
                type: FinancialTransactionType.Expense,
                createdAt: createdAt1,
                updatedAt: updatedAt1,
                categoryName: "Groceries"),

            FinancialTransactionFactory.Create(
                id: transactionId2,
                categoryId: categoryId2,
                userId: "user-1",
                amount: 3000m,
                comment: "Roller coaster",
                type: FinancialTransactionType.Income,
                createdAt: createdAt2,
                updatedAt: updatedAt2,
                categoryName: "Entertainment"),
        };

        [Fact]
        public void Projection_MatchesExistingTransaction_ReturnsMappedDto()
        {
            // Arrange
            var spec = new AdminFinancialTransactionByIdSpecification(transactionId1);

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            var dto = Assert.Single(result);
            Assert.Equal(transactionId1, dto.Id);
            Assert.Equal(categoryId1, dto.CategoryId);
            Assert.Equal(150.50m, dto.Amount);
            Assert.Equal("Apple", dto.Comment);
            Assert.Equal("Groceries", dto.CategoryName);
            Assert.Equal(FinancialTransactionType.Expense, dto.Type);
            Assert.Equal("user-1", dto.AppUserId);
            Assert.Equal(createdAt1, dto.CreatedAt);
            Assert.Equal(updatedAt1, dto.UpdatedAt);
        }

        [Fact]
        public void Id_DoesNotMatch_ReturnsEmpty()
        {
            // Arrange
            var spec = new AdminFinancialTransactionByIdSpecification(nonExistentId);

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            Assert.Empty(result);
        }
    }
}
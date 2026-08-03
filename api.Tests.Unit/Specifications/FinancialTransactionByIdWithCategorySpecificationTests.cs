using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Models;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class FinancialTransactionByIdWithCategorySpecificationTests
    {
        private static readonly Guid transactionId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid transactionId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        private static readonly Guid nonExistentId = Guid.Parse("00000000-0000-0000-0000-000000000099");

        private static readonly Guid categoryId1 = Guid.Parse("00000000-0000-0000-0000-000000000101");

        private static readonly DateTimeOffset createdAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt1 = new(2027, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly List<FinancialTransaction> _transactions = new()
        {
            FinancialTransactionFactory.Create(
                id: transactionId1,
                userId: "user-1",
                categoryId: categoryId1,
                categoryName: "Groceries",
                amount: 150.50m,
                type: FinancialTransactionType.Expense,
                comment: "Test1",
                createdAt: createdAt1,
                updatedAt: updatedAt1),

            FinancialTransactionFactory.Create(
                id: transactionId2,
                userId: "user-2",
                categoryId: categoryId1,
                categoryName: "Salary",
                amount: 3000m,
                type: FinancialTransactionType.Income,
                comment: "Test2",
                createdAt: createdAt1,
                updatedAt: updatedAt1),
        };

        [Fact]
        public void Id_DoesNotMatchAnyTransaction_ReturnsEmpty()
        {
            // Arrange
            var spec = new FinancialTransactionByIdWithCategorySpecification(nonExistentId, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void UserId_DoesNotMatchOwner_ReturnsEmpty()
        {
            // Arrange
            var spec = new FinancialTransactionByIdWithCategorySpecification(transactionId2, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Projection_MatchesOwnedTransaction_ReturnsMappedDto()
        {
            // Arrange
            var spec = new FinancialTransactionByIdWithCategorySpecification(transactionId1, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            var dto = Assert.Single(result);

            Assert.IsType<FinancialTransactionOutputDto>(dto);
            Assert.Equal(transactionId1, dto.Id);
            Assert.Equal(150.50m, dto.Amount);
            Assert.Equal(categoryId1, dto.CategoryId);
            Assert.Equal("Groceries", dto.CategoryName);
            Assert.Equal(FinancialTransactionType.Expense, dto.Type);
            Assert.Equal("Test1", dto.Comment);
            Assert.Equal(createdAt1, dto.CreatedAt);
            Assert.Equal(updatedAt1, dto.UpdatedAt);
        }
    }
}

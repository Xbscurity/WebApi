using api.Models;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class HasFinancialTransactionByCategoryIdSpecificationTests
    {
        private static readonly Guid categoryId1 = Guid.Parse("00000000-0000-0000-0000-000000000101");
        private static readonly Guid categoryId2 = Guid.Parse("00000000-0000-0000-0000-000000000102");
        private static readonly Guid nonExistentCategoryId = Guid.Parse("00000000-0000-0000-0000-000000000199");

        private readonly List<FinancialTransaction> _transactions = new()
        {
            FinancialTransactionFactory.Create(categoryId: categoryId1, userId: "user-1"),
            FinancialTransactionFactory.Create(categoryId: categoryId1, userId: "user-1"),
            FinancialTransactionFactory.Create(categoryId: categoryId2, userId: "user-1"),
        };

        [Fact]
        public void CategoryId_MatchesExistingTransactions_ReturnsAllMatchingTransactions()
        {
            // Arrange
            var spec = new HasFinancialTransactionsByCategoryIdSpecification(categoryId1);

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(categoryId1, r.CategoryId));
        }

        [Fact]
        public void CategoryId_HasNoRelatedTransactions_ReturnsEmpty()
        {
            // Arrange
            var spec = new HasFinancialTransactionsByCategoryIdSpecification(nonExistentCategoryId);

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            Assert.Empty(result);
        }
    }
}

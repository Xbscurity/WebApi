using api.Enums;
using api.Models;
using api.Queries;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class FinancialTransactionSortedPagedSpecificationTests
    {
        private static readonly Guid guid1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid guid2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid guid3 = Guid.Parse("00000000-0000-0000-0000-000000000003");
        private static readonly Guid guid4 = Guid.Parse("00000000-0000-0000-0000-000000000004");
        private static readonly Guid guid5 = Guid.Parse("00000000-0000-0000-0000-000000000005");

        private static readonly Guid categoryGuid1 = Guid.Parse("00000000-0000-0000-0000-000000000101");
        private static readonly Guid categoryGuid2 = Guid.Parse("00000000-0000-0000-0000-000000000102");
        private static readonly Guid categoryGuid3 = Guid.Parse("00000000-0000-0000-0000-000000000103");
        private static readonly Guid categoryGuid4 = Guid.Parse("00000000-0000-0000-0000-000000000104");
        private static readonly Guid categoryGuid5 = Guid.Parse("00000000-0000-0000-0000-000000000105");

        private static readonly DateTimeOffset createdAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt2 = new(2026, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt3 = new(2026, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt4 = new(2026, 1, 4, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt5 = new(2026, 1, 5, 1, 0, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset updatedAt1 = new(2027, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt2 = new(2027, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt3 = new(2027, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt4 = new(2027, 1, 4, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt5 = new(2027, 1, 5, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly List<FinancialTransaction> _transactions = new()
        {
            FinancialTransactionFactory.Create(
                id: guid1,
                userId: "user-1",
                categoryId: categoryGuid1,
                categoryName: "AAA",
                isActive: true,
                type: FinancialTransactionType.Income,
                comment: "Test1",
                amount: 100m,
                createdAt: createdAt1,
                updatedAt: updatedAt1),

            FinancialTransactionFactory.Create(
                id: guid2,
                userId: "user-1",
                categoryId: categoryGuid2,
                categoryName: "AAA",
                isActive: false,
                type: FinancialTransactionType.Expense,
                comment: "Test2",
                amount: 200m,
                createdAt: createdAt2,
                updatedAt: updatedAt2),

            FinancialTransactionFactory.Create(
                id: guid3,
                userId: "user-1",
                categoryId: categoryGuid3,
                categoryName: "BBB",
                isActive: true,
                type: FinancialTransactionType.Income,
                comment: "Test3",
                amount: 50m,
                createdAt: createdAt3,
                updatedAt: updatedAt3),

            FinancialTransactionFactory.Create(
                id: guid4,
                userId: "user-1",
                categoryId: categoryGuid4,
                categoryName: "CCC",
                isActive: true,
                type: FinancialTransactionType.Expense,
                comment: "Test4",
                amount: 300m,
                createdAt: createdAt4,
                updatedAt: updatedAt4),

            FinancialTransactionFactory.Create(
                id: guid5,
                userId: "user-2",
                categoryId: categoryGuid5,
                categoryName: "AAA",
                isActive: true,
                type: FinancialTransactionType.Income,
                comment: "Test5",
                amount: 500m,
                createdAt: createdAt5,
                updatedAt: updatedAt5),
        };

        [Fact]
        public void UserId_AlwaysApplied_ExcludesOtherUsersTransactions()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            Assert.Equal(_transactions.Count(x => x.AppUserId == "user-1"), result.Count);

            Assert.DoesNotContain(guid5, result);
        }

        [Fact]
        public void UserId_DoesNotMatchAnyTransaction_ReturnsEmpty()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-999");

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void IncludeInactive_IsFalse_ReturnsOnlyActiveCategoryTransactions()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = false,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            Assert.Equal(_transactions.Count(x => x.AppUserId == "user-1" && x.Category.IsActive), result.Count);
            Assert.All(result, r => Assert.True(
                _transactions.Single(t => t.Id == r.Id).Category.IsActive));
        }

        [Fact]
        public void StartDate_IsProvided_ReturnsTransactionsFromThatDateOnwards()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                StartDate = createdAt3,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid3, guid4 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void EndDate_IsProvided_ReturnsTransactionsUpToThatDate()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                EndDate = createdAt2,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid1, guid2 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void BothDates_AreProvided_ReturnsTransactionsWithinDateRange()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                StartDate = createdAt2,
                EndDate = createdAt3,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid2, guid3 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void SortBy_Category_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "category",
                IsDescending = false,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid2, guid1, guid3, guid4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Category_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "category",
                IsDescending = true,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid4, guid3, guid2, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Category_IsCaseInsensitive_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "CATEGORY",
                IsDescending = false,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid2, guid1, guid3, guid4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_HasWhitespace_TrimsAndSortsCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "  category  ",
                IsDescending = false,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid2, guid1, guid3, guid4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_IsNull_DefaultsToCreatedAtAndSortsCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = null!,
                IsDescending = true,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid4, guid3, guid2, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Amount_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "amount",
                IsDescending = false,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid3, guid1, guid2, guid4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Amount_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "amount",
                IsDescending = true,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid4, guid2, guid1, guid3 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Default_Ascending_OrdersByCreatedAt()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "any",
                IsDescending = false,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid1, guid2, guid3, guid4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Default_Descending_OrdersByCreatedAt()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "any",
                IsDescending = true,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid4, guid3, guid2, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Pagination_FirstPage_ReturnsCorrectNumberOfTransactions()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 2,
                IncludeInactive = true,
                SortBy = "createdat",
                IsDescending = false,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid1, guid2 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Pagination_LastPage_ReturnsRemainingTransactions()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 2,
                Size = 2,
                IncludeInactive = true,
                SortBy = "createdat",
                IsDescending = false,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid3, guid4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Projection_Select_ReturnsMappedDto()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 1,
                IncludeInactive = true,
                SortBy = "createdat",
                IsDescending = false,
            };
            var spec = new FinancialTransactionSortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            var dto = Assert.Single(result);
            Assert.Equal(guid1, dto.Id);
            Assert.Equal(100m, dto.Amount);
            Assert.Equal(categoryGuid1, dto.CategoryId);
            Assert.Equal("AAA", dto.CategoryName);
            Assert.Equal(FinancialTransactionType.Income, dto.Type);
            Assert.Equal("Test1", dto.Comment);
            Assert.Equal(createdAt1, dto.CreatedAt);
            Assert.Equal(updatedAt1, dto.UpdatedAt);
        }
    }
}
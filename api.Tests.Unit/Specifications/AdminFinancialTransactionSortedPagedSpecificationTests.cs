using api.Enums;
using api.Models;
using api.Queries;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class AdminFinancialTransactionSortedPagedSpecificationTests
    {
        private static readonly Guid ftId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid ftId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid ftId3 = Guid.Parse("00000000-0000-0000-0000-000000000003");
        private static readonly Guid ftId4 = Guid.Parse("00000000-0000-0000-0000-000000000004");
        private static readonly Guid ftId5 = Guid.Parse("00000000-0000-0000-0000-000000000005");
        private static readonly Guid ftId6 = Guid.Parse("00000000-0000-0000-0000-000000000006");
        private static readonly Guid ftId7 = Guid.Parse("00000000-0000-0000-0000-000000000007");

        private static readonly Guid categoryId1 = Guid.Parse("00000000-0000-0000-0000-000000000101");
        private static readonly Guid categoryId2 = Guid.Parse("00000000-0000-0000-0000-000000000102");
        private static readonly Guid categoryId3 = Guid.Parse("00000000-0000-0000-0000-000000000103");
        private static readonly Guid categoryId4 = Guid.Parse("00000000-0000-0000-0000-000000000104");
        private static readonly Guid categoryId5 = Guid.Parse("00000000-0000-0000-0000-000000000105");
        private static readonly Guid categoryId6 = Guid.Parse("00000000-0000-0000-0000-000000000106");
        private static readonly Guid categoryId7 = Guid.Parse("00000000-0000-0000-0000-000000000107");

        private static readonly DateTimeOffset createdAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt2 = new(2026, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt3 = new(2026, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt4 = new(2026, 2, 4, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt5 = new(2026, 2, 5, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt6 = new(2026, 2, 6, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt7 = new(2026, 2, 7, 1, 0, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset updatedAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt2 = new(2026, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt3 = new(2026, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt4 = new(2026, 2, 4, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt5 = new(2026, 2, 5, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt6 = new(2026, 2, 6, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt7 = new(2026, 2, 7, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly List<FinancialTransaction> _financialTransactions = new()
        {
            FinancialTransactionFactory
            .Create(
                id: ftId1,
                userId: "user-1",
                categoryId: categoryId1,
                categoryName: "AAA",
                isActive: true,
                amount: 100m,
                createdAt: createdAt1,
                updatedAt: updatedAt1,
                comment: "Test1"),

            FinancialTransactionFactory
            .Create(
                id: ftId2,
                userId: "user-1",
                categoryId: categoryId2,
                categoryName: "AAA",
                isActive: false,
                amount: 200m,
                createdAt: createdAt2,
                updatedAt: updatedAt2,
                comment: "Test2"),

            FinancialTransactionFactory
            .Create(
                id: ftId3,
                userId: "user-1",
                categoryId: categoryId3,
                categoryName: "BBB",
                isActive: true,
                amount: 50m,
                createdAt: createdAt3,
                updatedAt: updatedAt3,
                comment: "Test3"),

            FinancialTransactionFactory
            .Create(
                id: ftId4,
                userId: "user-2",
                categoryId: categoryId4,
                categoryName: "AAA",
                isActive: true,
                amount: 300m,
                createdAt: createdAt4,
                updatedAt: updatedAt4,
                comment: "Test4"),

            FinancialTransactionFactory
            .Create(
                id: ftId5,
                userId: "user-2",
                categoryId: categoryId5,
                categoryName: "AAA",
                isActive: false,
                amount: 10m,
                createdAt: createdAt5,
                updatedAt: updatedAt5,
                comment: "Test5"),

            FinancialTransactionFactory
            .Create(
                id: ftId6,
                userId: "user-2",
                categoryId: categoryId6,
                categoryName: "CCC",
                isActive: true,
                amount: 500m,
                createdAt: createdAt6,
                updatedAt: updatedAt6,
                comment: "Test6"),

            FinancialTransactionFactory
            .Create(
                id: ftId7,
                userId: "user-3",
                categoryId: categoryId7,
                categoryName: "CCC",
                isActive: true,
                amount: 150m,
                createdAt: createdAt7,
                updatedAt: updatedAt7,
                comment: "Test7"),
        };

        [Fact]
        public void Filters_NoneApplied_ReturnsAllTransactions()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).ToList();

            // Assert
            Assert.Equal(_financialTransactions.Count, result.Count);
        }

        [Theory]
        [InlineData("user-1")]
        [InlineData("user-2")]
        [InlineData("user-3")]
        public void UserId_IsProvided_AppliesFilter(string userId)
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                UserId = userId,
            };

            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).ToList();

            // Assert
            Assert.Equal(_financialTransactions.Count(x => x.AppUserId == query.UserId), result.Count);
            Assert.All(result, r => Assert.Equal(userId, r.AppUserId));
        }

        [Fact]
        public void IncludeInactive_IsFalse_FiltersByRelatedCategoryIsActive()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = false
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { ftId1, ftId3, ftId4, ftId6, ftId7 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void StartDate_IsProvided_ReturnsCategoriesFromThatDateOnwards()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "createdat",
                StartDate = createdAt4
            };

            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { ftId4, ftId5, ftId6, ftId7 };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EndDate_IsProvided_ReturnsCategoriesUpToThatDate()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "createdat",
                EndDate = createdAt3
            };

            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { ftId1, ftId2, ftId3 };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void BothDates_AreProvided_ReturnsCategoriesWithinDateRange()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "createdat",
                StartDate = createdAt2,
                EndDate = createdAt5
            };

            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { ftId2, ftId3, ftId4, ftId5 };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SortBy_Default_HasWhitespace_TrimsAndSortsCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "  createdat  ",
                IsDescending = false
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { ftId1, ftId2, ftId3, ftId4, ftId5, ftId6, ftId7 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Default_IsCaseInsensitive_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "CREATEDAT",
                IsDescending = false
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { ftId1, ftId2, ftId3, ftId4, ftId5, ftId6, ftId7 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_IsNull_DefaultsToCreatedAtAndOrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = null!,
                IsDescending = true
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            var expected = new[] { ftId7, ftId6, ftId5, ftId4, ftId3, ftId2, ftId1 };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SortBy_Category_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "category",
                IsDescending = false
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { ftId5, ftId4, ftId2, ftId1, ftId3, ftId7, ftId6 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Category_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "category",
                IsDescending = true
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { ftId7, ftId6, ftId3, ftId5, ftId4, ftId2, ftId1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Amount_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "amount",
                IsDescending = false
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { ftId5, ftId3, ftId1, ftId7, ftId2, ftId4, ftId6 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Amount_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "amount",
                IsDescending = true
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { ftId6, ftId4, ftId2, ftId7, ftId1, ftId3, ftId5 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Default_Ascending_DefaultsToCreatedAtAndOrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                IsDescending = false,
            };

            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { ftId1, ftId2, ftId3, ftId4, ftId5, ftId6, ftId7 };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SortBy_Default_Descending_DefaultsToCreatedAtAndOrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                IsDescending = true,
            };

            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { ftId7, ftId6, ftId5, ftId4, ftId3, ftId2, ftId1 };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Pagination_FirstPage_ReturnsCorrectNumberOfTransactions()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 3,
                IncludeInactive = true,
                SortBy = "createdat",
                IsDescending = false
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { ftId1, ftId2, ftId3 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Pagination_SecondPage_SkipsPreviousTransactionsAndTakesNext()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 2,
                Size = 3,
                IncludeInactive = true,
                SortBy = "createdat",
                IsDescending = false
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            var expectedOrder = new[] { ftId4, ftId5, ftId6 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Pagination_LastPage_ReturnsSingleTransaction()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 3,
                Size = 3,
                IncludeInactive = true,
                SortBy = "createdat",
                IsDescending = false
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).Select(r => r.Id).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(ftId7, result[0]);
        }

        [Fact]
        public void Projection_Select_ReturnsMappedDto()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 1,
                IncludeInactive = true,
                SortBy = "createdat",
                IsDescending = false
            };
            var spec = new AdminFinancialTransactionSortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_financialTransactions).ToList();

            // Assert
            var dto = Assert.Single(result);
            Assert.Equal(ftId1, dto.Id);
            Assert.Equal(100m, dto.Amount);
            Assert.Equal(categoryId1, dto.CategoryId);
            Assert.Equal(FinancialTransactionType.Income, dto.Type);
            Assert.Equal("AAA", dto.CategoryName);
            Assert.Equal("user-1", dto.AppUserId);
            Assert.Equal(createdAt1, dto.CreatedAt);
            Assert.Equal(updatedAt1, dto.UpdatedAt);
            Assert.Equal("Test1", dto.Comment);
        }
    }
}
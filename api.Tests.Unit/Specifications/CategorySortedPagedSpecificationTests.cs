using api.Models;
using api.Queries;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class CategorySortedPagedSpecificationTests
    {
        private static readonly Guid guid1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid guid2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid guid3 = Guid.Parse("00000000-0000-0000-0000-000000000003");
        private static readonly Guid guid4 = Guid.Parse("00000000-0000-0000-0000-000000000004");
        private static readonly Guid guid5 = Guid.Parse("00000000-0000-0000-0000-000000000005");

        private static readonly DateTimeOffset createdAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt2 = new(2026, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt3 = new(2026, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt4 = new(2026, 2, 4, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt5 = new(2026, 2, 5, 1, 0, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset updatedAt1 = new(2027, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt2 = new(2027, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt3 = new(2027, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt4 = new(2027, 2, 4, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt5 = new(2027, 2, 5, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly List<Category> _categories = new()
        {
            CategoryFactory.Create(
                id: guid1,
                userId: "user-1",
                name: "AAA",
                isActive: true,
                createdAt: createdAt1,
                updatedAt: updatedAt1),

            CategoryFactory.Create(
                id: guid2,
                userId: "user-1",
                name: "AAA",
                isActive: false,
                createdAt: createdAt2,
                updatedAt: updatedAt2),

            CategoryFactory.Create(
                id: guid3,
                userId: "user-1",
                name: "BBB",
                isActive: true,
                createdAt: createdAt3,
                updatedAt: updatedAt3),

            CategoryFactory.Create(
                id: guid4,
                userId: "user-1",
                name: "CCC",
                isActive: true,
                createdAt: createdAt4,
                updatedAt: updatedAt4),

            CategoryFactory.Create(
                id: guid5,
                userId: "user-2",
                name: "AAA",
                isActive: true,
                createdAt: createdAt5,
                updatedAt: updatedAt5),
        };

        [Fact]
        public void UserId_AlwaysApplied_ReturnsOnlyOwnCategories()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(r => r.Id).ToList();

            // Assert
            Assert.Equal(_categories.Count(x => x.AppUserId == "user-1"), result.Count);
            Assert.DoesNotContain(guid5, result);
        }

        [Fact]
        public void IncludeInactive_IsFalse_ReturnsOnlyActiveCategories()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = false
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).ToList();

            // Assert
            Assert.Equal(_categories.Count(x => x.AppUserId == "user-1" && x.IsActive), result.Count);
            Assert.All(result, r => Assert.True(r.IsActive));
        }

        [Fact]
        public void StartDate_IsProvided_ReturnsCategoriesFromThatDateOnwards()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                StartDate = createdAt3
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid3, guid4 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void EndDate_IsProvided_ReturnsCategoriesUpToThatDate()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                EndDate = createdAt2
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid1, guid2 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void BothDates_AreProvided_ReturnsCategoriesWithinDateRange()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                StartDate = createdAt2,
                EndDate = createdAt3
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid2, guid3 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void SortBy_Name_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "name",
                IsDescending = false
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid2, guid1, guid3, guid4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Name_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "name",
                IsDescending = true
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid4, guid3, guid2, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Name_IsCaseInsensitive_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "NAME",
                IsDescending = false
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

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
                SortBy = "  name  ",
                IsDescending = false
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

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
                IsDescending = true
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid4, guid3, guid2, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_IsActive_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "isactive",
                IsDescending = false
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid2, guid4, guid3, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_IsActive_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "isactive",
                IsDescending = true
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid4, guid3, guid1, guid2 };
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
                IsDescending = false
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

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
                IsDescending = true
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid4, guid3, guid2, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Pagination_FirstPage_ReturnsCorrectNumberOfCategories()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 1,
                Size = 2,
                IncludeInactive = true,
                SortBy = "createdat",
                IsDescending = false
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid1, guid2 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Pagination_LastPage_ReturnsRemainingCategories()
        {
            // Arrange
            var query = new EntityQuery
            {
                Page = 2,
                Size = 2,
                IncludeInactive = true,
                SortBy = "createdat",
                IsDescending = false
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

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
                IsDescending = false
            };
            var spec = new CategorySortedPagedSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_categories).ToList();

            // Assert
            var dto = Assert.Single(result);
            Assert.Equal(guid1, dto.Id);
            Assert.Equal("AAA", dto.Name);
            Assert.True(dto.IsActive);
            Assert.Equal(createdAt1, dto.CreatedAt);
            Assert.Equal(updatedAt1, dto.UpdatedAt);
        }
    }
}

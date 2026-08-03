using api.Dtos.Category;
using api.Models;
using api.Queries;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class AdminCategorySortedPagedSpecificationTests
    {
        private static readonly Guid guid1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid guid2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid guid3 = Guid.Parse("00000000-0000-0000-0000-000000000003");
        private static readonly Guid guid4 = Guid.Parse("00000000-0000-0000-0000-000000000004");
        private static readonly Guid guid5 = Guid.Parse("00000000-0000-0000-0000-000000000005");
        private static readonly Guid guid6 = Guid.Parse("00000000-0000-0000-0000-000000000006");
        private static readonly Guid guid7 = Guid.Parse("00000000-0000-0000-0000-000000000007");

        private static readonly DateTimeOffset createdAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt2 = new(2026, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt3 = new(2026, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt4 = new(2026, 2, 4, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt5 = new(2026, 2, 5, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt6 = new(2026, 2, 6, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt7 = new(2026, 2, 7, 1, 0, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset updatedAt1 = new(2027, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt2 = new(2027, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt3 = new(2027, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt4 = new(2027, 2, 4, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt5 = new(2027, 2, 5, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt6 = new(2027, 2, 6, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt7 = new(2027, 2, 7, 1, 0, 0, 0, TimeSpan.Zero);

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
                userId: "user-2",
                name: "AAA",
                isActive: true,
                createdAt: createdAt4,
                updatedAt: updatedAt4),

            CategoryFactory.Create(
                id: guid5,
                userId: "user-2",
                name: "AAA",
                isActive: false,
                createdAt: createdAt5,
                updatedAt: updatedAt5),

            CategoryFactory.Create(
                id: guid6,
                userId: "user-2",
                name: "CCC",
                isActive: true,
                createdAt: createdAt6,
                updatedAt : updatedAt6),

            CategoryFactory.Create(
                id: guid7,
                userId: "user-3",
                name: "CCC",
                isActive: true,
                createdAt: createdAt7,
                updatedAt : updatedAt7),
        };

        [Fact]
        public void Filters_NoneApplied_ReturnsAllCategories()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
            };

            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(r => r.Id).ToList();

            // Assert
            Assert.Equal(_categories.Count, result.Count);
            Assert.Equal(
                _categories.Select(c => c.Id).OrderBy(id => id),
                result.OrderBy(id => id));
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

            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).ToList();

            // Assert
            Assert.Equal(_categories.Count(x => x.AppUserId == query.UserId), result.Count);
            Assert.All(result, r => Assert.Equal(userId, r.AppUserId));
        }

        [Fact]
        public void IncludeInactive_IsFalse_ReturnsOnlyActiveCategories()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = false,
            };

            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid1, guid3, guid4, guid6, guid7 };

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
                StartDate = createdAt4
            };

            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid4, guid5, guid6, guid7 };

            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
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
                EndDate = createdAt3
            };

            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid1, guid2, guid3 };

            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
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
                StartDate = createdAt2,
                EndDate = createdAt5
            };

            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(x => x.Id).ToList();

            // Assert
            var expected = new[] { guid2, guid3, guid4, guid5 };

            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void SortBy_Name_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "name",
                IsDescending = false
            };
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid5, guid4, guid2, guid1, guid3, guid7, guid6 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_HasWhitespace_TrimsAndSortsCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "  name  ",
                IsDescending = false
            };
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid5, guid4, guid2, guid1, guid3, guid7, guid6 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Name_IsCaseInsensitive_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "NAME",
                IsDescending = false
            };
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid5, guid4, guid2, guid1, guid3, guid7, guid6 };
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
                IsDescending = false
            };
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid1, guid2, guid3, guid4, guid5, guid6, guid7 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Name_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "name",
                IsDescending = true
            };
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();
            // Assert
            var expectedOrder = new[] { guid7, guid6, guid3, guid5, guid4, guid2, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_IsActive_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "isactive",
                IsDescending = false
            };
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid5, guid2, guid7, guid6, guid4, guid3, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_IsActive_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new AdminEntityQuery
            {
                Page = 1,
                Size = 10,
                IncludeInactive = true,
                SortBy = "isactive",
                IsDescending = true
            };
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid7, guid6, guid4, guid3, guid1, guid5, guid2 };
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
                IsDescending = false
            };
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid1, guid2, guid3, guid4, guid5, guid6, guid7 };
            Assert.Equal(expectedOrder, result);
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
                IsDescending = true
            };
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).Select(c => c.Id).ToList();

            // Assert
            var expectedOrder = new[] { guid7, guid6, guid5, guid4, guid3, guid2, guid1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Pagination_FirstPage_ReturnsCorrectNumberOfCategories()
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
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).ToList();

            // Assert
            var expectedOrder = new[] { guid1, guid2, guid3 };
            Assert.Equal(expectedOrder, result.Select(r => r.Id));
        }

        [Fact]
        public void Pagination_SecondPage_SkipsPreviousCategoriesAndTakesNext()
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
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).ToList();

            // Assert
            var expectedOrder = new[] { guid4, guid5, guid6 };
            Assert.Equal(expectedOrder, result.Select(r => r.Id));
        }


        [Fact]
        public void Pagination_LastPage_ReturnsSingleCategory()
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
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(guid7, result[0].Id);
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
            var spec = new AdminCategorySortedPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_categories).ToList();

            // Assert
            var dto = Assert.Single(result);

            Assert.IsType<AdminCategoryOutputDto>(dto);

            Assert.Equal(guid1, dto.Id);
            Assert.Equal("AAA", dto.Name);
            Assert.True(dto.IsActive);
            Assert.Equal("user-1", dto.AppUserId);
            Assert.Equal(createdAt1, dto.CreatedAt);
            Assert.Equal(updatedAt1, dto.UpdatedAt);
        }
    }
}
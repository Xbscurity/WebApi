using api.Models;
using api.Queries;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class UserManagementPagedSpecificationTests
    {
        private static readonly string id1 = "user-1";
        private static readonly string id2 = "user-2";
        private static readonly string id3 = "user-3";
        private static readonly string id4 = "user-4";

        private static readonly DateTimeOffset createdAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt2 = new(2026, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt3 = new(2026, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt4 = new(2026, 1, 4, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly List<AppUser> _users = new()
        {
            AppUserFactory.Create(
                id: id1,
                userName: "bob",
                email: "bob@test.com",
                isBanned: false,
                createdAt: createdAt1),

            AppUserFactory.Create(
                id: id2,
                userName: "alice",
                email: "alice@test.com",
                isBanned: true,
                createdAt: createdAt2),

            AppUserFactory.Create(
                id: id3,
                userName: "charlie",
                email: "zzz@test.com",
                isBanned: false,
                createdAt: createdAt3),

            AppUserFactory.Create(
                id: id4,
                userName: "dave",
                email: "dddd@test.com",
                isBanned: false,
                createdAt: createdAt4),
        };

        [Fact]
        public void NoFilters_ReturnsAllUsers()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).ToList();

            // Assert
            Assert.Equal(_users.Count, result.Count);
        }

        [Fact]
        public void SortBy_UserName_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "username",
                IsDescending = false,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id2, id1, id3, id4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_UserName_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "username",
                IsDescending = true,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id4, id3, id1, id2 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_UserName_IsCaseInsensitive_OrdersCorrectly()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "USERNAME",
                IsDescending = false,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id2, id1, id3, id4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_HasWhitespace_TrimsAndSortsCorrectly()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "  username  ",
                IsDescending = false,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id2, id1, id3, id4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_IsNull_DefaultsToCreatedAtAndSortsCorrectly()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = null!,
                IsDescending = true,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id4, id3, id2, id1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Email_Ascending_OrdersCorrectly()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "email",
                IsDescending = false,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id2, id1, id4, id3 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Email_Descending_OrdersCorrectly()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "email",
                IsDescending = true,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id3, id4, id1, id2 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_IsBanned_Ascending_PutsFalseFirst()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "isbanned",
                IsDescending = false,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id4, id3, id1, id2 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_IsBanned_Descending_PutsTrueFirst()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "isbanned",
                IsDescending = true,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id2, id4, id3, id1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Default_Ascending_OrdersByCreatedAt()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "any",
                IsDescending = false,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id1, id2, id3, id4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void SortBy_Default_Descending_OrdersByCreatedAt()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 10,
                SortBy = "any",
                IsDescending = true,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id4, id3, id2, id1 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Pagination_FirstPage_ReturnsCorrectNumberOfUsers()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 2,
                SortBy = "createdat",
                IsDescending = false,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id1, id2 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Pagination_LastPage_ReturnsRemainingUsers()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 2,
                Size = 2,
                SortBy = "createdat",
                IsDescending = false,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).Select(u => u.Id).ToList();

            // Assert
            var expectedOrder = new[] { id3, id4 };
            Assert.Equal(expectedOrder, result);
        }

        [Fact]
        public void Projection_Select_ReturnsMappedDto()
        {
            // Arrange
            var query = new UserManagementQuery
            {
                Page = 1,
                Size = 1,
                SortBy = "createdat",
                IsDescending = false,
            };
            var spec = new UserManagementPagedSpecification(query);

            // Act
            var result = spec.Evaluate(_users).ToList();

            // Assert
            var dto = Assert.Single(result);
            Assert.Equal(id1, dto.Id);
            Assert.Equal("bob@test.com", dto.Email);
            Assert.Equal("bob", dto.UserName);
            Assert.False(dto.IsBanned);
            Assert.Equal(createdAt1, dto.CreatedAt);
        }
    }
}

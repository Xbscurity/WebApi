using api.Dtos.Category;
using api.Models;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class CategoryByIdSpecificationTests
    {
        private static readonly Guid guid1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid guid2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid nonExistentGuid = Guid.Parse("00000000-0000-0000-0000-000000000099");

        private static readonly DateTimeOffset createdAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt2 = new(2026, 1, 1, 2, 0, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset updatedAt1 = new(2027, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset updatedAt2 = new(2027, 1, 1, 2, 0, 0, 0, TimeSpan.Zero);

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
                userId: "user-2",
                name: "BBB",
                isActive: false,
                createdAt: createdAt2,
                updatedAt: updatedAt2),
        };

        [Fact]
        public void Id_DoesNotMatch_ReturnsEmpty()
        {
            // Arrange
            var spec = new CategoryByIdSpecification(nonExistentGuid, "user-1");

            // Act
            var result = spec.Evaluate(_categories).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void UserId_DoesNotMatch_ReturnsEmpty()
        {
            // Arrange
            var spec = new CategoryByIdSpecification(guid2, "user-1");

            // Act
            var result = spec.Evaluate(_categories).ToList();

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Projection_MatchesExistingCategory_ReturnsMappedDto()
        {
            // Arrange
            var spec = new CategoryByIdSpecification(guid1, "user-1");

            // Act
            var result = spec.Evaluate(_categories).ToList();

            // Assert
            var dto = Assert.Single(result);

            Assert.IsType<CategoryOutputDto>(dto);
            Assert.Equal(guid1, dto.Id);
            Assert.Equal("AAA", dto.Name);
            Assert.True(dto.IsActive);
            Assert.Equal(createdAt1, dto.CreatedAt);
            Assert.Equal(updatedAt1, dto.UpdatedAt);
        }
    }
}

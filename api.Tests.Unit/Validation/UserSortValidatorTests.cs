using api.Validation;

namespace api.Tests.Unit.Validation
{
    public class UserSortValidatorTests
    {
        private readonly UserSortValidator _validator = new();

        [Theory]
        [InlineData(null, true)]
        [InlineData("id", true)]
        [InlineData("username", true)]
        [InlineData("email", true)]
        [InlineData("isbanned", true)]
        [InlineData("Id", true)]
        [InlineData("invalid", false)]
        public void IsValid_ReturnsExpectedResult(string? field, bool expected)
        {
            // Act
            var result = _validator.IsValid(field);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
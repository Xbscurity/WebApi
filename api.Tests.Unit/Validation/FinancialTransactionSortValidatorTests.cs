using api.Validation;

namespace api.Tests.Unit.Validation
{
    public class FinancialTransactionSortValidatorTests
    {
        private readonly FinancialTransactionSortValidator _validator = new();

        [Theory]
        [InlineData(null, true)]
        [InlineData("id", true)]
        [InlineData("category", true)]
        [InlineData("amount", true)]
        [InlineData("date", true)]
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
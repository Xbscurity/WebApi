using api.Responses;
using api.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Tests.Unit.Filters
{
    public class ExceptionFilterTests
    {
        private readonly ExceptionFilter _filter;
        public ExceptionFilterTests()
        {
            _filter = new ExceptionFilter();
        }
        [Fact]
        public void OnException_UnhandledException_ReturnsApiResponse500()
        {
            // Arrange
            var context = new ExceptionContext(FilterTestHelper.CreateActionContext(), new List<IFilterMetadata>())
            {
                Exception = new Exception("Test exception")
            };

            // Act
            _filter.OnException(context);

            // Assert
            var result = Assert.IsType<ObjectResult>(context.Result);
            var response = Assert.IsType<ApiResponse>(result.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.NotNull(response.Error);
            Assert.Equal("INTERNAL_ERROR", response.Error.Code);
            Assert.Equal("Server error occured", response.Error.Message);
            Assert.True(context.ExceptionHandled);
        }
    }
}

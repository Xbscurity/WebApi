using api.Filters;
using api.Helpers;
using api.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Tests.Unit.Filters
{
    public class StatusCodeFilterTests
    {
        private static ResultExecutingContext CreateContext(string errorCode)
        {
            var response = new ApiResponse
            {
                Error = new Error { Code = errorCode }
            };

            var result = new ObjectResult(response);

            return new ResultExecutingContext(
                FilterTestHelper.CreateActionContext(),
                new List<IFilterMetadata>(),
                result,
                controller: null);
        }
        [Theory]
        [InlineData("NOT_FOUND", StatusCodes.Status404NotFound)]
        [InlineData("VALIDATION_ERROR", StatusCodes.Status422UnprocessableEntity)]
        [InlineData("BAD_REQUEST", StatusCodes.Status400BadRequest)]
        [InlineData("UNKNOWN_CODE", StatusCodes.Status200OK)]
        [InlineData(null, StatusCodes.Status200OK)]
        public void OnResultExecuting_ErrorCode_SetsExpectedStatusCode(string errorCode, int expectedStatusCode)
        {
            var filter = new StatusCodeFilter();
            // Arrange
            var context = CreateContext(errorCode);

            // Act
            filter.OnResultExecuting(context);

            // Assert
            Assert.Equal(expectedStatusCode, context.HttpContext.Response.StatusCode);
        }
    }
}

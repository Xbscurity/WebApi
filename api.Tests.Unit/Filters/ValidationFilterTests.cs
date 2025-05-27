using api.Filters;
using api.Helpers;
using api.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace api.Tests.Unit.Filters
{
    public class ValidationFilterTests
    {
        private static ActionExecutingContext CreateContext(bool isValid, Dictionary<string, string[]>? modelErrors = null)
        {
            var httpContext = new DefaultHttpContext();
            var modelState = new ModelStateDictionary();

            if (!isValid && modelErrors != null)
            {
                foreach (var kvp in modelErrors)
                {
                    foreach (var error in kvp.Value)
                        modelState.AddModelError(kvp.Key, error);
                }
            }

            var actionContext = FilterTestHelper.CreateActionContext();
            foreach (var kvp in modelState)
            {
                foreach (var error in kvp.Value.Errors)
                {
                    actionContext.ModelState.AddModelError(kvp.Key, error.ErrorMessage);
                }
            }

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller: null
            );
        }

        [Fact]
        public void OnActionExecuting_InvalidModelState_SetsBadRequestWithApiResponse()
        {
            // Arrange
            var filter = new ValidationFilter();
            var modelErrors = new Dictionary<string, string[]>
            {
                { "Name", new[] { "Name is required" } },
                { "Age", new[] { "Age must be positive" } }
            };
            var context = CreateContext(false, modelErrors);

            // Act
            filter.OnActionExecuting(context);

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(context.Result);
            var response = Assert.IsType<ApiResponse<object>>(result.Value);

            Assert.Equal("VALIDATION_ERROR", response.Error?.Code);
            Assert.Equal("Validation failed", response.Error?.Message);

            var errorData = Assert.IsType<Dictionary<string, List<string>>>(response.Error?.Data);

            Assert.Equal(2, errorData.Count);
            Assert.Contains("Name", errorData.Keys);
            Assert.Contains("Age", errorData.Keys);
            Assert.Contains("Name is required", errorData["Name"]);
            Assert.Contains("Age must be positive", errorData["Age"]);
        }

        [Fact]
        public void OnActionExecuting_ValidModelState_DoesNothing()
        {
            // Arrange
            var filter = new ValidationFilter();
            var context = CreateContext(true);

            // Act
            filter.OnActionExecuting(context);

            // Assert
            Assert.Null(context.Result);
        }
    }
}

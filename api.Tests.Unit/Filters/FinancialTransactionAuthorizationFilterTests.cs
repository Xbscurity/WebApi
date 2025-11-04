using api.Constants;
using api.Filters;
using api.Models;
using api.Providers.Interfaces;
using api.Repositories.Interfaces;
using api.Responses;
using api.Services.Transaction;
using api.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace api.Tests.Unit.Filters
{
    public class FinancialTransactionAuthorizationFilterTests
    {
        private readonly Mock<IAuthorizationService> _authServiceMock;
        private readonly Mock<ILogger<FinancialTransactionAuthorizationFilter>> _loggerMock;
        private readonly Mock<IFinancialTransactionRepository> _transactionServiceMock;

        private readonly FinancialTransactionAuthorizationFilter _filter;
        private readonly Mock<ITimeProvider> _timeProviderMock;

        public FinancialTransactionAuthorizationFilterTests()
        {
            _timeProviderMock = new Mock<ITimeProvider>();

            _timeProviderMock.Setup(t => t.UtcNow).Returns(new DateTimeOffset(2025, 10, 9, 10, 0, 0, TimeSpan.Zero));

            _authServiceMock = new Mock<IAuthorizationService>();
            _loggerMock = new Mock<ILogger<FinancialTransactionAuthorizationFilter>>();
            _transactionServiceMock = new Mock<IFinancialTransactionRepository>();

            _filter = new FinancialTransactionAuthorizationFilter(
                _authServiceMock.Object,
                _loggerMock.Object,
                _transactionServiceMock.Object
            );
        }

        private static ActionExecutingContext CreateContext(object? id = null)
        {
            var actionContext = FilterTestHelper.CreateActionContext();

            var actionArguments = new Dictionary<string, object?>();
            if (id != null)
                actionArguments["id"] = id;

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments,
                controller: null
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("not_an_int")]
        public async Task OnActionExecutionAsync_InvalidOrMissingId_ReturnsBadRequest(object? id)
        {
            // Arrange
            var context = CreateContext(id);
            var next = new Mock<ActionExecutionDelegate>();

            // Act
            await _filter.OnActionExecutionAsync(context, next.Object);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);

            _transactionServiceMock.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _authServiceMock.Verify(a => a.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()),
                Times.Never
            );
            next.Verify(x => x(), Times.Never);
        }

        [Fact]
        public async Task OnActionExecutionAsync_TransactionNotFound_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = 999;
            var context = CreateContext(nonExistingId);

            _transactionServiceMock.Setup(s => s.GetByIdAsync(nonExistingId)).ReturnsAsync((FinancialTransaction?)null);

            var next = new Mock<ActionExecutionDelegate>();

            // Act
            await _filter.OnActionExecutionAsync(context, next.Object);

            // Assert
            Assert.IsType<NotFoundObjectResult>(context.Result);

            _transactionServiceMock.Verify(x => x.GetByIdAsync(nonExistingId), Times.Once);

            _authServiceMock.Verify(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()),
                Times.Never
            );
            next.Verify(x => x(), Times.Never);
        }

        [Fact]
        public async Task OnActionExecutionAsync_AuthorizationFails_ReturnsForbidden()
        {
            // Arrange
            var transaction = new FinancialTransaction(_timeProviderMock.Object) { Id = 1 };
            var context = CreateContext(transaction.Id);

            _transactionServiceMock.Setup(s => s.GetByIdAsync(transaction.Id)).ReturnsAsync(transaction);

            var failedAuthResult = AuthorizationResult.Failed();
            _authServiceMock.Setup(a => a.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                transaction,
                Policies.TransactionAccess))
                .ReturnsAsync(failedAuthResult);

            var next = new Mock<ActionExecutionDelegate>();

            // Act
            await _filter.OnActionExecutionAsync(context, next.Object);

            // Assert
            _transactionServiceMock.Verify(x => x.GetByIdAsync(transaction.Id), Times.Once);
            _authServiceMock.Verify(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                transaction,
                Policies.TransactionAccess),
                Times.Once
            );
            next.Verify(x => x(), Times.Never);

            var objectResult = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, objectResult.StatusCode);

            var apiResponse = Assert.IsType<ApiResponse<object>>(objectResult.Value);
            Assert.Equal("Forbidden access to financial transaction.", apiResponse.Error.Message);
        }

        [Fact]
        public async Task OnActionExecutionAsync_AuthorizationSucceeds_CallsNextDelegate()
        {
            // Arrange
            var transaction = new FinancialTransaction(_timeProviderMock.Object)
            {
                Id = 1,
                Category = new Category
                {
                    IsActive = false
                }
            };
            var context = CreateContext(transaction.Id);

            _transactionServiceMock.Setup(s => s.GetByIdAsync(transaction.Id)).ReturnsAsync(transaction);

            var successfulAuthResult = AuthorizationResult.Success();
            _authServiceMock
                .Setup(a => a.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    transaction,
                    Policies.TransactionAccess))
                .ReturnsAsync(successfulAuthResult);

            var next = new Mock<ActionExecutionDelegate>();

            // Act
            await _filter.OnActionExecutionAsync(context, next.Object);

            // Assert
            _transactionServiceMock.Verify(x => x.GetByIdAsync(transaction.Id), Times.Once);
            _authServiceMock.Verify(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                transaction,
                Policies.TransactionAccess), Times.Once);

            next.Verify(x => x(), Times.Once);
            Assert.Null(context.Result);
        }
    }
}

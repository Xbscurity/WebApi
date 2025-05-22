using api.Dtos.FinancialTransactions;
using api.Models;
using api.Providers.Interfaces;
using api.Repositories.Interfaces;
using api.Services.Interfaces;
using api.Services.Transaction;
using Moq;

namespace api.Tests.Unit.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ITimeProvider> _timeStub;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();

            _timeStub = new Mock<ITimeProvider>();
            var fixedTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
            _timeStub.Setup(t => t.UtcNow).Returns(fixedTime);

            var strategiesStub = new List<IGroupingReportStrategy> { };
            _transactionService = new TransactionService(_transactionRepositoryMock.Object, _timeStub.Object, strategiesStub);
        }

        [Fact]
        public async Task UpdateAsync_CategoryNotExists_ReturnsNull()
        {
            // Arrange
            const int notExistingCategoryId = 999;
            var inputDto = new FinancialTransactionInputDto { Comment = "Anything" };
            _transactionRepositoryMock
                .Setup(r => r.GetByIdAsync(notExistingCategoryId))
                .ReturnsAsync((FinancialTransaction?)null);

            // Act
            var result = await _transactionService.UpdateAsync(notExistingCategoryId, inputDto);

            // Assert
            Assert.Null(result);
            _transactionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FinancialTransaction>()), Times.Never);

        }

        [Fact]
        public async Task UpdateAsync_CategoryExists_ReturnsUpdatedCategory()
        {
            // Arrange
            const int existingTransactionId = 1;
            var receivedTransaction = new FinancialTransaction(_timeStub.Object) { Id = existingTransactionId, Comment = "Test" };
            var inputDto = new FinancialTransactionInputDto { Comment = "Updated" };

            _transactionRepositoryMock
                .Setup(r => r.GetByIdAsync(existingTransactionId))
                .ReturnsAsync(receivedTransaction);

            _transactionRepositoryMock
                .Setup(r => r.UpdateAsync(It.Is<FinancialTransaction>(t => t.Id == existingTransactionId && t.Comment == "Updated")))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _transactionService.UpdateAsync(existingTransactionId, inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result!.Comment);
            Assert.Equal(existingTransactionId, result.Id);
            _transactionRepositoryMock.Verify(r => r.UpdateAsync(
             It.Is<FinancialTransaction>(t => t.Id == existingTransactionId && t.Comment == "Updated")),
             Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CategoryNotExists_ReturnsFalse()
        {
            // Arrange
            const int notExistingFinancialTransactionId = 999;
            _transactionRepositoryMock
                .Setup(r => r.GetByIdAsync(notExistingFinancialTransactionId))
                .ReturnsAsync((FinancialTransaction?)null);

            // Act
            var result = await _transactionService.DeleteAsync(notExistingFinancialTransactionId);

            // Assert
            Assert.False(result);
            _transactionRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<FinancialTransaction>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_CategoryExists_ReturnsUpdatedCategory()
        {
            // Arrange
            const int existingFinancialTransactionId = 1;
            var receivedCategory = new FinancialTransaction(_timeStub.Object)
            { Id = existingFinancialTransactionId, Comment = "Test" };
            _transactionRepositoryMock
                .Setup(r => r.GetByIdAsync(existingFinancialTransactionId))
                .ReturnsAsync(receivedCategory);

            _transactionRepositoryMock
                .Setup(r => r.DeleteAsync(receivedCategory))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _transactionService.DeleteAsync(existingFinancialTransactionId);

            // Assert
            Assert.True(result);
            _transactionRepositoryMock.Verify(r => r.DeleteAsync(receivedCategory),
             Times.Once);
        }


    }
}

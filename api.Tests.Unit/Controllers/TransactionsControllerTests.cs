using api.Controllers;
using api.Dtos;
using api.Enums;
using api.Models;
using api.Repositories.Interfaces;
using Moq;

namespace api.Tests.Controllers
{
    public class TransactionsControllerTests
    {
        private readonly Mock<ITransactionRepository> _repositoryMock;
        private readonly Mock<ITimeProvider> _timeStub;
        private readonly TransactionController _controller;
        public TransactionsControllerTests()
        {
            _repositoryMock = new Mock<ITransactionRepository>();
            _timeStub = new Mock<ITimeProvider>();
            var fixedTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
            _timeStub.Setup(t => t.UtcNow).Returns(fixedTime);

            _controller = new TransactionController(_repositoryMock.Object, _timeStub.Object);
        }

        [Theory]
        [MemberData(nameof(GetAllTransactionsTestDataAndCount))]
        public async Task GetAll_ListOfTransactions_ReturnsExpectedCount(List<FinancialTransaction> financialTransactions, int expectedCount)
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(financialTransactions);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.Equal(expectedCount, result.Data.Count);
            Assert.Null(result.Error);
        }
        public static IEnumerable<object[]> GetAllTransactionsTestDataAndCount()
        {
            var fixedTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

            var timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock.Setup(t => t.UtcNow).Returns(fixedTime);
            yield return new object[]
            {
                new List<FinancialTransaction>
                {
                    new FinancialTransaction(timeProviderMock.Object),
                    new FinancialTransaction(timeProviderMock.Object)
                },
                2
            };
            yield return new object[] { new List<FinancialTransaction>(), 0 };
        }
        [Fact]
        public async Task GetById_TransactionExists_ReturnsOkWithCategory()
        {
            // Arrange
            int existingFinancialTransactionId = 1;
            var existingFinancialTransaction = new FinancialTransaction(_timeStub.Object) { Id = existingFinancialTransactionId };
            _repositoryMock.Setup(r => r.GetByIdAsync(existingFinancialTransactionId)).ReturnsAsync(existingFinancialTransaction);

            // Act
            var result = await _controller.GetById(existingFinancialTransactionId);

            // Assert
            Assert.Equal(existingFinancialTransaction.Id, result.Data.Id);
            Assert.Null(result.Error);
        }
        [Fact]
        public async Task GetById_TransactionNotExists_ReturnsNotFound()
        {
            // Arrange
            int notExistingFinancialTransactionId = 999;
            _repositoryMock.Setup(r => r.GetByIdAsync(notExistingFinancialTransactionId)).ReturnsAsync((FinancialTransaction?)null);

            // Act
            var result = await _controller.GetById(notExistingFinancialTransactionId);

            // Assert
            Assert.Equal("Transaction not found", result.Error.Message);
            Assert.Equal("NOT_FOUND", result.Error.Code);
        }
        [Fact]
        public async Task Update_TransactionExists_ReturnsOkWithUpdatedTransaction()
        {
            // Arrange
            const int existingFinancialTransactionId = 1;
            var financialTransactionDto = new FinancialTransactionInputDto
            { CategoryId = 1, Amount = 100, Comment = "test" };
            var receivedFinancialTransaction = new FinancialTransaction(_timeStub.Object) { Id = existingFinancialTransactionId };
            var financialTransactionToUpdate = new FinancialTransaction(_timeStub.Object)
            {
                Id = existingFinancialTransactionId,
                CategoryId = financialTransactionDto.CategoryId,
                Amount = financialTransactionDto.Amount,
                Comment = financialTransactionDto.Comment
            };
            _repositoryMock.Setup(r => r.GetByIdAsync(existingFinancialTransactionId)).ReturnsAsync(receivedFinancialTransaction);
            _repositoryMock.Setup(r => r.UpdateAsync(financialTransactionToUpdate)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(existingFinancialTransactionId, financialTransactionDto);

            // Assert
            Assert.Equivalent(financialTransactionToUpdate, result.Data);
            Assert.Null(result.Error);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<FinancialTransaction>(t =>
                t.Amount == financialTransactionDto.Amount &&
                t.Comment == financialTransactionDto.Comment &&
                t.CategoryId == financialTransactionDto.CategoryId
            )), Times.Once);
        }
        [Fact]
        public async Task Update_TransactionNotExists_ReturnsNotFound()
        {
            // Arrange
            const int notExistingFinancialTransactionId = 999;
            var financialTransactionDto = new FinancialTransactionInputDto
            { CategoryId = 1, Amount = 100, Comment = "test" };
            _repositoryMock.Setup(t => t.GetByIdAsync(notExistingFinancialTransactionId)).ReturnsAsync((FinancialTransaction?)null);

            // Act
            var result = await _controller.Update(notExistingFinancialTransactionId, financialTransactionDto);

            // Assert
            Assert.Equal("NOT_FOUND", result.Error.Code);
            Assert.Equal("Transaction not found", result.Error.Message);
        }
        [Fact]
        public async Task Delete_TransactionExists_ReturnsOkWithTrue()
        {
            // Arrange
            const int existingFinancialTransactionId = 1;
            var receivedFinancialTransaction = new FinancialTransaction(_timeStub.Object)
            { CategoryId = 1, Amount = 100, Comment = "test" };
            _repositoryMock.Setup(r => r.GetByIdAsync(existingFinancialTransactionId)).ReturnsAsync(receivedFinancialTransaction);
            _repositoryMock.Setup(r => r.DeleteAsync(receivedFinancialTransaction)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(existingFinancialTransactionId);

            // Assert
            Assert.Null(result.Error);
            Assert.True(result.Data);
            _repositoryMock.Verify(r => r.DeleteAsync(receivedFinancialTransaction), Times.Once);
        }
        [Fact]
        public async Task Delete_TransactionNotExists_ReturnNotFound()
        {
            // Arrange
            const int notExistingFinancialTransactionId = 999;
            _repositoryMock.Setup(r => r.GetByIdAsync(notExistingFinancialTransactionId)).ReturnsAsync((FinancialTransaction?)null);
            // Act
            var result = await _controller.Delete(notExistingFinancialTransactionId);

            // Assert
            Assert.Equal("Transaction not found", result.Error.Message);
            Assert.Equal("NOT_FOUND", result.Error.Code);
        }
        [Fact]
        public async Task GetReport_ReportTypeCategory_CallsGetReportByCategoryAsync()
        {
            // Arrange
            GroupingReportStrategyKey reportType = GroupingReportStrategyKey.ByCategory;

            // Act
            await _controller.GetReport(null, reportType);

            // Assert
            _repositoryMock.Verify(r => r.GetReportByCategoryAsync(null), Times.Once);

        }
        [Fact]
        public async Task GetReport_ReportTypeDate_CallsGetReportByDateAsync()
        {
            // Arrange
            GroupingReportStrategyKey reportType = GroupingReportStrategyKey.ByDate;

            // Act
            await _controller.GetReport(null, reportType);

            // Assert
            _repositoryMock.Verify(r => r.GetReportByDateAsync(null), Times.Once);

        }
        [Fact]
        public async Task GetReport_ReportTypeCategoryAndDate_CallsGetReportByCategoryAndDateAsync()
        {
            // Arrange
            GroupingReportStrategyKey reportType = GroupingReportStrategyKey.ByCategoryAndDate;

            // Act
            await _controller.GetReport(null, reportType);

            // Assert
            _repositoryMock.Verify(r => r.GetReportByCategoryAndDateAsync(null), Times.Once);

        }
    }
}

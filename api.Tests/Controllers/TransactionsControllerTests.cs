using api.Controllers;
using api.Dtos;
using api.Helpers;
using api.Helpers.Report;
using api.Models;
using api.Repositories.Interfaces;
using Moq;

namespace api.Tests.Controllers
{
    public class TransactionsControllerTests
    {
        private readonly Mock<ITransactionsRepository> _repositoryMock;
        private readonly TransactionsController _controller;
        public TransactionsControllerTests()
        {
            _repositoryMock = new Mock<ITransactionsRepository>();
            _controller = new TransactionsController(_repositoryMock.Object);
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
            yield return new object[]
            {
                new List<FinancialTransaction>
                {
                    new FinancialTransaction(),
                    new FinancialTransaction()
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
            var existingFinancialTransaction = new FinancialTransaction { Id = existingFinancialTransactionId };
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
            var financialTransactionDto = new FinancialTransactionDto
            { CategoryId = 1, Amount = 100, Comment = "test" };
            var receivedFinancialTransaction = new FinancialTransaction { Id = existingFinancialTransactionId };
            var financialTransactionToUpdate = new FinancialTransaction
            { 
                Id = existingFinancialTransactionId, CategoryId = financialTransactionDto.CategoryId,
                Amount = financialTransactionDto.Amount, Comment = financialTransactionDto.Comment                  
            };
            _repositoryMock.Setup(r => r.GetByIdAsync(existingFinancialTransactionId)).ReturnsAsync(receivedFinancialTransaction);
            _repositoryMock.Setup(r => r.UpdateAsync(financialTransactionToUpdate)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(existingFinancialTransactionId, financialTransactionDto);

            // Assert
            Assert.Equivalent(financialTransactionToUpdate, result.Data);
            Assert.Null(result.Error);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<FinancialTransaction>(t => FinancialTransactionsAreEqual(financialTransactionToUpdate, t))), Times.Once);
        }
        [Fact]
        public async Task Update_TransactionNotExists_ReturnsNotFound()
        {
            // Arrange
            const int notExistingFinancialTransactionId = 999;
            var financialTransactionDto = new FinancialTransactionDto
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
            var receivedFinancialTransaction = new FinancialTransaction()
            {CategoryId = 1, Amount = 100, Comment = "test" };
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
        private bool FinancialTransactionsAreEqual(FinancialTransaction expected, FinancialTransaction actual)
        {
            return expected.CategoryId == actual.CategoryId && expected.Amount == actual.Amount && expected.Comment == actual.Comment;
        }
    }
}

using api.Controllers;
using api.Dtos.FinancialTransactions;
using api.Helpers;
using api.QueryObjects;
using api.Services.Transaction;
using Moq;

namespace api.Tests.Unit.Controllers
{
    public class TransactionControllerTests
    {
        private readonly Mock<ITransactionService> _serviceMock;
        private readonly UserTransactionController _controller;

        public TransactionControllerTests()
        {
            _serviceMock = new Mock<ITransactionService>();
            _controller = new UserTransactionController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_InvalidSortByField_ReturnsBadRequest()
        {
            // Arrange
            var paginationQueryObject = new PaginationQueryObject
            {
                SortBy = "InvalidField"
            };

            // Act
            var result = await _controller.GetAll(paginationQueryObject);

            // Assert
            Assert.Equal("BAD_REQUEST", result.Error?.Code);
        }
        [Theory]
        [InlineData("Id")]
        [InlineData("Category")]
        [InlineData("Amount")]
        [InlineData("date")]
        public async Task GetAll_ValidSortByField_ReturnsOk(string sortBy)
        {
            // Arrange
            var paginationQueryObject = new PaginationQueryObject
            {
                SortBy = sortBy
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(paginationQueryObject))
                .ReturnsAsync(new PagedData<BaseFinancialTransactionOutputDto>());

            // Act
            var result = await _controller.GetAll(paginationQueryObject);

            // Assert
            Assert.Null(result.Error);
        }
        [Fact]
        public async Task GetById_TransactionExists_ReturnsOkWithCategory()
        {
            // Arrange
            int existingFinancialTransactionId = 1;
            var existingFinancialTransaction = new BaseFinancialTransactionOutputDto()
            { Id = existingFinancialTransactionId };
            _serviceMock
                .Setup(s => s.GetByIdAsync(existingFinancialTransactionId))
                .ReturnsAsync(existingFinancialTransaction);

            // Act
            var result = await _controller.GetById(existingFinancialTransactionId);

            // Assert
            Assert.Null(result.Error);
            Assert.Equal(existingFinancialTransaction.Id, result.Data.Id);
        }
        [Fact]
        public async Task GetById_TransactionNotExists_ReturnsNotFound()
        {
            // Arrange
            int notExistingFinancialTransactionId = 999;
            _serviceMock
                .Setup(s => s.GetByIdAsync(notExistingFinancialTransactionId))
                .ReturnsAsync((BaseFinancialTransactionOutputDto?)null);

            // Act
            var result = await _controller.GetById(notExistingFinancialTransactionId);

            // Assert
            Assert.Null(result.Data);
            Assert.Equal("Transaction not found", result.Error?.Message);
            Assert.Equal("NOT_FOUND", result.Error?.Code);
        }
        [Fact]
        public async Task Update_TransactionExists_ReturnsOkWithUpdatedTransaction()
        {
            // Arrange
            const int existingFinancialTransactionId = 1;
            var financialTransactionInputDto = new BaseFinancialTransactionInputDto
            { Comment = "test" };
            var updatedFinancialTransactionOutputDto = new BaseFinancialTransactionOutputDto()
            {
                Id = existingFinancialTransactionId,
                Comment = financialTransactionInputDto.Comment
            };
            _serviceMock
                .Setup(s => s.UpdateAsync(existingFinancialTransactionId, financialTransactionInputDto))
                .ReturnsAsync(updatedFinancialTransactionOutputDto);

            // Act
            var result = await _controller.Update(existingFinancialTransactionId, financialTransactionInputDto);

            // Assert
            Assert.Null(result.Error);
            Assert.Equal(updatedFinancialTransactionOutputDto, result.Data);
        }
        [Fact]
        public async Task Update_TransactionNotExists_ReturnsNotFound()
        {
            // Arrange
            const int notExistingFinancialTransactionId = 999;
            var financialTransactionInputDto = new BaseFinancialTransactionInputDto
            { Comment = "test" };
            _serviceMock
                .Setup(t => t.UpdateAsync(notExistingFinancialTransactionId, financialTransactionInputDto))
                .ReturnsAsync((BaseFinancialTransactionOutputDto?)null);

            // Act
            var result = await _controller.Update(notExistingFinancialTransactionId, financialTransactionInputDto);

            // Assert
            Assert.Null(result.Data);
            Assert.Equal("NOT_FOUND", result.Error?.Code);
            Assert.Equal("Transaction not found", result.Error?.Message);
        }
        [Fact]
        public async Task Delete_TransactionExists_ReturnsOkWithTrue()
        {
            // Arrange
            const int existingFinancialTransactionId = 1;
            _serviceMock
                .Setup(s => s.DeleteAsync(existingFinancialTransactionId))
                .ReturnsAsync(true);
            // Act
            var result = await _controller.Delete(existingFinancialTransactionId);

            // Assert
            Assert.Null(result.Error);
            Assert.True(result.Data);
        }
        [Fact]
        public async Task Delete_TransactionNotExists_ReturnNotFound()
        {
            // Arrange
            const int notExistingFinancialTransactionId = 999;
            _serviceMock
                .Setup(s => s.DeleteAsync(notExistingFinancialTransactionId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(notExistingFinancialTransactionId);

            // Assert
            Assert.Equal("Transaction not found", result.Error?.Message);
            Assert.Equal("NOT_FOUND", result.Error?.Code);
            _serviceMock.Verify(s => s.DeleteAsync(notExistingFinancialTransactionId), Times.Once());
        }

        [Fact]
        public async Task GetReport_InvalidSortByField_ReturnsBadRequest()
        {
            // Arrange
            var paginationQueryObject = new PaginationQueryObject
            {
                SortBy = "InvalidField"
            };
            // Act
            var result = await _controller.GetAll(paginationQueryObject);

            // Assert
            Assert.Equal("BAD_REQUEST", result.Error?.Code);
        }
        [Theory]
        [InlineData("Id")]
        [InlineData("Category")]
        [InlineData("Amount")]
        [InlineData("date")]
        public async Task GetReport_ValidSortByField_ReturnsOk(string sortBy)
        {
            // Arrange
            var reportQueryObject = new ReportQueryObject
            {
                SortBy = sortBy
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(reportQueryObject))
                .ReturnsAsync(new PagedData<BaseFinancialTransactionOutputDto>());

            // Act
            var result = await _controller.GetAll(reportQueryObject);

            // Assert
            Assert.Null(result.Error);
        }
    }
}

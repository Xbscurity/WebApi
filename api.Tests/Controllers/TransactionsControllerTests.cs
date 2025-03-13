using api.Controllers;
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
        [Fact]
        public async Task GetAll_ListOfTransactions_ReturnsOkWithTransactions()
        {
            // Arrange
            var transactions = new List<FinancialTransaction>
            {
                new FinancialTransaction{Id = 2, Amount = -100 },
                new FinancialTransaction{Id = 1, Amount = 100 },
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(transactions);
            
            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Null(result.Error);

        }
    }
}

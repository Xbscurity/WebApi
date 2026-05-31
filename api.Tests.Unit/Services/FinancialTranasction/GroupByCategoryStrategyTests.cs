using api.Models;
using api.Services.FinancialTransactions;
using MockQueryable;
using Moq;

namespace api.Tests.Unit.Services.FinancialTranasction
{
    public class GroupByCategoryStrategyTests
    {
        private readonly Mock<ITimeProvider> _timeStub;
        private readonly GroupByCategoryStrategy _strategy;
        public GroupByCategoryStrategyTests()
        {
            _timeStub = new Mock<ITimeProvider>();
            _strategy = new GroupByCategoryStrategy();
        }
        [Fact]
        public async Task GroupAsync_ValidInput_ReturnsCorrectGroupedData()
        {
            // Arrange
            FinancialTransaction NewFt(int id, string category) =>
                new FinancialTransaction(Mock.Of<ITimeProvider>())
                {
                    Id = id,
                    Category = new Category
                    {
                        Name = category,
                    }
                };

            var data = new List<FinancialTransaction>
            {
                NewFt(1, "Food "),
                NewFt(2, "   food "),
                NewFt(3, "Food"),

                NewFt(4, "   Pills  "),
                NewFt(5, "Pills"),

                NewFt(6, "Entertainment"),
            }.AsQueryable();

            var mock = data.BuildMock();

            // Act
            var result = await _strategy.GroupAsync(mock);

            // Assert
            Assert.Equal(3, result.Count);

            Assert.Contains(result, g => g.Key.Category == "food");

            var foodGroup = result.Single(g => g.Key.Category == "food");
            Assert.Equal(3, foodGroup.Transactions.Count);

            var foodIds = foodGroup.Transactions.Select(t => t.Id).ToList();
            Assert.Contains(1, foodIds);
            Assert.Contains(2, foodIds);
            Assert.Contains(3, foodIds);


            Assert.Contains(result, g => g.Key.Category == "pills");
            var pillsGroup = result.First(g => g.Key.Category == "pills");
            Assert.Equal(2, pillsGroup.Transactions.Count);

            var pillsIds = pillsGroup.Transactions.Select(t => t.Id).ToList();
            Assert.Contains(4, pillsIds);
            Assert.Contains(5, pillsIds);

            Assert.Contains(result, g => g.Key.Category == "entertainment");
            var entertainmentGroup = result.Single(g => g.Key.Category == "entertainment");
            Assert.Single(entertainmentGroup.Transactions);

            var entertainmentIds = entertainmentGroup.Transactions.Select(t => t.Id).ToList();
            Assert.Contains(6, entertainmentIds);
        }

        [Fact]
        public async Task GroupAsync_CheckMapping_ReturnsCorrectMapping()
        {
            // Arrange
            var ft = new FinancialTransaction(_timeStub.Object)
            {
                Id = 1,
                AppUserId = "user-123",
                Amount = 100,
                CategoryId = 1,
                Comment = "Test",
                Category = new Category
                {
                    Name = "Food"
                }
            };

            var data = new List<FinancialTransaction> { ft }.AsQueryable();

            var mock = data.BuildMock();

            // Act
            var result = await _strategy.GroupAsync(mock);

            // Assert
            var expectedKey = ft.Category.Name.ToLowerInvariant().Trim();
            Assert.Equal(expectedKey, result.Single().Key.Category);

            var dto = result.Single().Transactions.Single();
            Assert.Equal(ft.Id, dto.Id);
            Assert.Equal(ft.Amount, dto.Amount);
            Assert.Equal(ft.Comment, dto.Comment);
            Assert.Equal(ft.AppUserId, dto.AppUserId);
            Assert.Equal(ft.CreatedAt, dto.CreatedAt);
            Assert.Equal(ft.Category.Name, dto.CategoryName);
        }
    }
}

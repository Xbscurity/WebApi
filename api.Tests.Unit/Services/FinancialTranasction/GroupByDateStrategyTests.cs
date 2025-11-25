using api.Models;
using api.Providers.Interfaces;
using api.Services.Transaction;
using MockQueryable;
using Moq;

namespace api.Tests.Unit.Services.FinancialTranasction
{
    public class GroupByDateStrategyTests
    {
        private readonly Mock<ITimeProvider> _timeStub;

        private readonly GroupByDateStrategy _strategy;
        public GroupByDateStrategyTests()
        {
            _timeStub = new Mock<ITimeProvider>();
            _strategy = new GroupByDateStrategy();
        }

        [Fact]
        public async Task GroupAsync_ValidInput_ReturnsCorrectGroupedData()
        {
            // Arrange

            FinancialTransaction NewFt(int id, int year, int month) =>
                new FinancialTransaction(Mock.Of<ITimeProvider>())
                {
                    Id = id,
                    CreatedAt = new DateTimeOffset(year, month, 1, 12, 0, 0, TimeSpan.Zero)
                };

            var data = new List<FinancialTransaction>
            {
                NewFt(1, 2025, 1),
                NewFt(2, 2025, 1),
                NewFt(3, 2025, 1),

                NewFt(4, 2025, 2),
                NewFt(5, 2025, 2),

                NewFt(6, 2026, 1)
            }.AsQueryable();

            var mock = data.BuildMock();

            // Act
            var result = await _strategy.GroupAsync(mock);

            // Assert
            Assert.Equal(3, result.Count);

            Assert.Contains(result, g => g.Key.Month == 1 && g.Key.Year == 2025);
            var january2025Group = result.Single(g => g.Key.Month == 1 && g.Key.Year == 2025);
            var january2025GroupIds = january2025Group.Transactions.Select(t => t.Id).ToList();
            Assert.Contains(1, january2025GroupIds);
            Assert.Contains(2, january2025GroupIds);
            Assert.Contains(3, january2025GroupIds);

            Assert.Contains(result, g => g.Key.Month == 2 && g.Key.Year == 2025);
            var february2025Group = result.Single(g => g.Key.Month == 2 && g.Key.Year == 2025);
            var february2025GroupIds = february2025Group.Transactions.Select(t => t.Id).ToList();
            Assert.Contains(4, february2025GroupIds);
            Assert.Contains(5, february2025GroupIds);

            Assert.Contains(result, g => g.Key.Month == 1 && g.Key.Year == 2026);
            var january2026Group = result.Single(g => g.Key.Month == 1 && g.Key.Year == 2026);
            var january2026GroupId = january2026Group.Transactions.Single().Id;
            Assert.Equal(6, january2026GroupId);
        }

        [Fact]
        public async Task GroupAsync_CheckMapping_ReturnsCorrectMapping()
        {
            // Arrange
            var fixedTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

            var ft = new FinancialTransaction(_timeStub.Object)
            {
                Id = 1,
                AppUserId = "user-123",
                Amount = 100,
                CategoryId = 1,
                Comment = "Test",
                CreatedAt = fixedTime,
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
            var expectedMonth = fixedTime.Month;
            var expectedYear = fixedTime.Year;
            Assert.Equal(expectedMonth, result.Single().Key.Month);
            Assert.Equal(expectedYear, result.Single().Key.Year);

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

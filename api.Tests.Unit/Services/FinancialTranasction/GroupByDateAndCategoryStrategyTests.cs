using api.Models;
using api.Providers.Interfaces;
using api.Services.Transaction;
using MockQueryable;
using Moq;
using System.Runtime.CompilerServices;

namespace api.Tests.Unit.Services.FinancialTranasction
{
    public class GroupByDateAndCategoryStrategyTests
    {
        private readonly GroupByDateAndCategoryStrategy _strategy;
        public GroupByDateAndCategoryStrategyTests()
        {
            _strategy = new GroupByDateAndCategoryStrategy();
        }

        [Fact]
        public async Task GroupAsync_ValidInput_ReturnsCorrectGroupedData()
        {
            // Arrange
            FinancialTransaction NewFt(int id, int year, int month, string category) =>
               new FinancialTransaction(Mock.Of<ITimeProvider>())
               {
                   Id = id,
                   CreatedAt = new DateTimeOffset(year, month, 1, 12, 0, 0, TimeSpan.Zero),
                   Category = new Category
                   {
                       Name = category,
                   }
               };
            var data = new List<FinancialTransaction>
            {
               NewFt(1, 2025, 1, "Food"),
               NewFt(2, 2025, 1, "Food"),

               NewFt(3, 2025, 1, "Pills"),
               NewFt(4, 2025, 1, "Pills"),

               NewFt(5, 2025, 2, "Food"),
               NewFt(6, 2025, 2, "Food"),

               NewFt(7, 2026, 1, "Food"),

               NewFt(8, 2026,1, "Entertainment"),
            }.AsQueryable();

            var mock = data.BuildMock();

            // Act
            var result = await _strategy.GroupAsync(mock);

            // Assert
            Assert.Equal(5, result.Count);

            Assert.Contains(
                result,
                g => g.Key.Category == "food" && g.Key.Month == 1 && g.Key.Year == 2025);
            var foodJanuary2025Group = result.Single(
                g => g.Key.Category == "food" && g.Key.Month == 1 && g.Key.Year == 2025);
            Assert.Equal(2, foodJanuary2025Group.Transactions.Count);
            Assert.Equal([1, 2], foodJanuary2025Group.Transactions.Select(t => t.Id));

            Assert.Contains(
                result,
                g => g.Key.Category == "pills" && g.Key.Month == 1 && g.Key.Year == 2025);
            var pillsJanuary2025Group = result.Single(
                g => g.Key.Category == "pills" && g.Key.Month == 1 && g.Key.Year == 2025);
            Assert.Equal(2, pillsJanuary2025Group.Transactions.Count);
            Assert.Equal([3, 4], pillsJanuary2025Group.Transactions.Select(t => t.Id));

            Assert.Contains(
                result,
                g => g.Key.Category == "food" && g.Key.Month == 2 && g.Key.Year == 2025);
            var foodFebruary2025Group = result.Single(
                g => g.Key.Category == "food" && g.Key.Month == 2 && g.Key.Year == 2025);
            Assert.Equal(2, foodFebruary2025Group.Transactions.Count);
            Assert.Equal([5, 6], foodFebruary2025Group.Transactions.Select(t => t.Id));

            Assert.Contains(
                result,
                g => g.Key.Category == "food" && g.Key.Month == 1 && g.Key.Year == 2026);
            var foodJanuary2026Group = result.Single(
                g => g.Key.Category == "food" && g.Key.Month == 1 && g.Key.Year == 2026);
            Assert.Single(foodJanuary2026Group.Transactions);
            Assert.Equal(7, foodJanuary2026Group.Transactions.Single().Id);

            Assert.Contains(
                result,
                g => g.Key.Category == "entertainment" && g.Key.Month == 1 && g.Key.Year == 2026);
            var entertainmentJanuary2026Group = result.Single(
                g => g.Key.Category == "entertainment" && g.Key.Month == 1 && g.Key.Year == 2026);
            Assert.Single(entertainmentJanuary2026Group.Transactions);
            Assert.Equal(8, entertainmentJanuary2026Group.Transactions.Single().Id);
        }
        [Fact]
        public async Task GroupAsync_CheckMapping_ReturnsCorrectMapping()
        {
            // Arrange
            var fixedTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
            var ft = new FinancialTransaction(Mock.Of<ITimeProvider>())
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
            var expectedKey = ft.Category.Name.ToLowerInvariant().Trim();
            Assert.Equal(expectedKey, result.Single().Key.Category);
            Assert.Equal(fixedTime.Year, result.Single().Key.Year);
            Assert.Equal(fixedTime.Month, result.Single().Key.Month);

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
//using api.Data;
//using api.Models;
//using api.Repositories;
//using Microsoft.EntityFrameworkCore;
//using Moq;

//namespace api.Tests.Unit.Repositories
//{
//    public class TransactionsRepositoryTests : IDisposable
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly TransactionRepository _repository;
//        private readonly Mock<ITimeProvider> _timeStub;
//        public TransactionsRepositoryTests()
//        {

//            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//            .UseInMemoryDatabase(Guid.NewGuid().ToString())
//            .Options;

//            _context = new ApplicationDbContext(options);
//            _repository = new TransactionRepository(_context);
//            _timeStub = new Mock<ITimeProvider>();
//            var fixedTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
//            _timeStub.Setup(t => t.UtcNow).Returns(fixedTime);

//            SeedDatabase();
//        }
//        private void SeedDatabase()
//        {
//            _context.Transactions.AddRange(
//                new FinancialTransaction(_timeStub.Object)
//                {
//                    Id = 1,
//                    Category = new Category { Name = "Food" },
//                    Amount = 100,
//                    CreatedAt = new DateTimeOffset(2023, 1, 10, 0, 0, 0, TimeSpan.Zero),
//                    Comment = "Grocery shopping"
//                },
//                new FinancialTransaction(_timeStub.Object)
//                {
//                    Id = 2,
//                    Category = new Category { Name = "Transport" },
//                    Amount = 50,
//                    CreatedAt = new DateTimeOffset(2023, 1, 15, 0, 0, 0, TimeSpan.Zero),
//                    Comment = "Bus ticket"
//                },

//                new FinancialTransaction(_timeStub.Object)
//                {
//                    Id = 3,
//                    Category = new Category { Name = "Food" },
//                    Amount = 150,
//                    CreatedAt = new DateTimeOffset(2024, 3, 2, 0, 0, 0, TimeSpan.Zero),
//                    Comment = "Dinner"
//                },
//                new FinancialTransaction(_timeStub.Object)
//                {
//                    Id = 4,
//                    Category = new Category { Name = "Transport" },
//                    Amount = 75,
//                    CreatedAt = new DateTimeOffset(2024, 3, 3, 0, 0, 0, TimeSpan.Zero),
//                    Comment = "Taxi"
//                },

//                new FinancialTransaction(_timeStub.Object)
//                {
//                    Id = 5,
//                    Category = new Category { Name = "Entertainment" },
//                    Amount = 200,
//                    CreatedAt = new DateTimeOffset(2024, 12, 5, 0, 0, 0, TimeSpan.Zero),
//                    Comment = "Cinema"
//                },

//                new FinancialTransaction(_timeStub.Object)
//                {
//                    Id = 6,
//                    Category = null,
//                    Amount = 30,
//                    CreatedAt = new DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero),
//                    Comment = "Unknown expense"
//                },
//                new FinancialTransaction(_timeStub.Object)
//                {
//                    Id = 7,
//                    Category = null,
//                    Amount = 40,
//                    CreatedAt = new DateTimeOffset(2025, 1, 6, 0, 0, 0, TimeSpan.Zero),
//                    Comment = "Misc"
//                }
//            );

//            _context.SaveChanges();
//        }
//        [Fact]
//        public async Task GetReportByCategoryAsync_NoMatchingTransactions_ReturnsEmptyList()
//        {
//            // Arrange
//            var dateRange = new ReportQueryObject
//            {
//                StartDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
//                EndDate = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero)
//            };

//            // Act
//            var result = await _repository.GetReportByCategoryAsync(dateRange);

//            // Assert
//            Assert.Empty(result);
//        }
//        [Fact]
//        public async Task GetReportByCategoryAsync_ValidDateRange_ReturnsAllGroupedResults()
//        {
//            // Arrange
//            var dateRange = new ReportQueryObject
//            {
//                StartDate = new DateTimeOffset(2023, 1, 10, 0, 0, 0, TimeSpan.Zero),
//                EndDate = new DateTimeOffset(2025, 1, 6, 0, 0, 0, TimeSpan.Zero)
//            };

//            // Act
//            var result = await _repository.GetReportByCategoryAsync(dateRange);

//            // Assert
//            Assert.Equal(4, result.Count);

//            var foodGroup = result.FirstOrDefault(r => r.Key.Category == "Food");
//            Assert.NotNull(foodGroup);
//            Assert.Equal(2, foodGroup.Transactions.Count);
//            Assert.Contains(foodGroup.Transactions, t => t.Comment == "Grocery shopping");
//            Assert.Contains(foodGroup.Transactions, t => t.Comment == "Dinner");

//            var transportGroup = result.FirstOrDefault(r => r.Key.Category == "Transport");
//            Assert.NotNull(transportGroup);
//            Assert.Equal(2, transportGroup.Transactions.Count);
//            Assert.Contains(transportGroup.Transactions, t => t.Comment == "Bus ticket");
//            Assert.Contains(transportGroup.Transactions, t => t.Comment == "Taxi");

//            var entertainmentGroup = result.FirstOrDefault(r => r.Key.Category == "Entertainment");
//            Assert.NotNull(entertainmentGroup);
//            Assert.Single(entertainmentGroup.Transactions);
//            Assert.Contains(entertainmentGroup.Transactions, t => t.Comment == "Cinema");

//            var noCategoryGroup = result.FirstOrDefault(r => r.Key.Category == "No category");
//            Assert.NotNull(noCategoryGroup);
//            Assert.Equal(2, noCategoryGroup.Transactions.Count);
//            Assert.Contains(noCategoryGroup.Transactions, t => t.Comment == "Unknown expense");
//            Assert.Contains(noCategoryGroup.Transactions, t => t.Comment == "Misc");
//        }

//        [Fact]
//        public async Task GetReportByCategoryAsync_FilteredByStartDate_ReturnsCorrectTransactions()
//        {
//            // Arrange
//            var dateRange = new ReportQueryObject
//            {
//                StartDate = new DateTimeOffset(2024, 12, 5, 0, 0, 0, TimeSpan.Zero)
//            };

//            // Act
//            var result = await _repository.GetReportByCategoryAsync(dateRange);

//            // Assert
//            Assert.Equal(2, result.Count);

//            var entertainmentGroup = result.FirstOrDefault(r => r.Key.Category == "Entertainment");
//            Assert.NotNull(entertainmentGroup);
//            Assert.Single(entertainmentGroup.Transactions);
//            Assert.Contains(entertainmentGroup.Transactions, t => t.Comment == "Cinema");

//            var noCategoryGroup = result.FirstOrDefault(r => r.Key.Category == "No category");
//            Assert.NotNull(noCategoryGroup);
//            Assert.Equal(2, noCategoryGroup.Transactions.Count);
//            Assert.Contains(noCategoryGroup.Transactions, t => t.Comment == "Unknown expense");
//            Assert.Contains(noCategoryGroup.Transactions, t => t.Comment == "Misc");
//        }
//        [Fact]
//        public async Task GetReportByDateAsync_NoMatchingTransactions_ReturnsEmptyList()
//        {
//            // Arrange
//            var dateRange = new ReportQueryObject
//            {
//                StartDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
//                EndDate = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero)
//            };

//            // Act
//            var result = await _repository.GetReportByDateAsync(dateRange);

//            // Assert
//            Assert.Empty(result);
//        }
//        [Fact]
//        public async Task GetReportByDateAsync_ValidDateRange_ReturnsAllGroupedResults()
//        {
//            // Arrange 
//            var dateRange = new ReportQueryObject
//            {
//                StartDate = new DateTimeOffset(2023, 1, 10, 0, 0, 0, TimeSpan.Zero),
//                EndDate = new DateTimeOffset(2025, 1, 6, 0, 0, 0, TimeSpan.Zero)
//            };
//            // Act
//            var result = await _repository.GetReportByDateAsync(dateRange);

//            // Assert
//            Assert.Equal(4, result.Count);

//            var year2023Month01Group = result.FirstOrDefault(result => result.Key.Year == 2023 && result.Key.Month == 1);
//            Assert.NotNull(year2023Month01Group);
//            Assert.Equal(2, year2023Month01Group.Transactions.Count);
//            Assert.Contains(year2023Month01Group.Transactions, t => t.Comment == "Grocery shopping");
//            Assert.Contains(year2023Month01Group.Transactions, t => t.Comment == "Bus ticket");

//            var year2024Month03Group = result.FirstOrDefault(result => result.Key.Year == 2024 && result.Key.Month == 3);
//            Assert.NotNull(year2024Month03Group);
//            Assert.Equal(2, year2024Month03Group.Transactions.Count);
//            Assert.Contains(year2024Month03Group.Transactions, t => t.Comment == "Dinner");
//            Assert.Contains(year2024Month03Group.Transactions, t => t.Comment == "Taxi");

//            var year2024Month12Group = result.FirstOrDefault(result => result.Key.Year == 2024 && result.Key.Month == 12);
//            Assert.NotNull(year2024Month12Group);
//            Assert.Single(year2024Month12Group.Transactions);
//            Assert.Contains(year2024Month12Group.Transactions, t => t.Comment == "Cinema");

//            var year2025Month01Group = result.FirstOrDefault(result => result.Key.Year == 2025 && result.Key.Month == 1);
//            Assert.NotNull(year2025Month01Group);
//            Assert.Equal(2, year2025Month01Group.Transactions.Count);
//            Assert.Contains(year2025Month01Group.Transactions, t => t.Comment == "Unknown expense");
//            Assert.Contains(year2025Month01Group.Transactions, t => t.Comment == "Misc");
//        }

//        [Fact]
//        public async Task GetReportByDateAsync_FilteredByStartDate_ReturnsCorrectTransactions()
//        {
//            // Arrange
//            var dateRange = new ReportQueryObject
//            {
//                StartDate = new DateTimeOffset(2024, 12, 5, 0, 0, 0, TimeSpan.Zero)
//            };

//            // Act
//            var result = await _repository.GetReportByDateAsync(dateRange);

//            // Assert

//            Assert.Equal(2, result.Count);

//            var year2024Month12Group = result.FirstOrDefault(result => result.Key.Year == 2024 && result.Key.Month == 12);
//            Assert.NotNull(year2024Month12Group);
//            Assert.Single(year2024Month12Group.Transactions);
//            Assert.Contains(year2024Month12Group.Transactions, t => t.Comment == "Cinema");

//            var year2025Month01Group = result.FirstOrDefault(result => result.Key.Year == 2025 && result.Key.Month == 01);
//            Assert.NotNull(year2025Month01Group);
//            Assert.Equal(2, year2025Month01Group.Transactions.Count);
//            Assert.Contains(year2025Month01Group.Transactions, t => t.Comment == "Unknown expense");
//            Assert.Contains(year2025Month01Group.Transactions, t => t.Comment == "Misc");
//        }
//        [Fact]
//        public async Task GetReportByCategoryAndDateAsync_NoMatchingTransactions_ReturnsEmptyList()
//        {
//            // Arrange
//            var dateRange = new ReportQueryObject
//            {
//                StartDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
//                EndDate = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero)
//            };

//            // Act
//            var result = await _repository.GetReportByCategoryAndDateAsync(dateRange);

//            // Assert
//            Assert.Empty(result);
//        }
//        [Fact]
//        public async Task GetReportByCategoryAndDateAsync_FilteredByStartDate_ReturnsCorrectTransactions()
//        {
//            // Arrange
//            var dateRange = new ReportQueryObject
//            {
//                StartDate = new DateTimeOffset(2023, 1, 10, 0, 0, 0, TimeSpan.Zero),
//                EndDate = new DateTimeOffset(2024, 3, 2, 0, 0, 0, TimeSpan.Zero)
//            };
//            // Act
//            var result = await _repository.GetReportByCategoryAndDateAsync(dateRange);

//            // Assert
//            Assert.Equal(3, result.Count);

//            var year2023Month01FoodGroup = result.FirstOrDefault(result => result.Key.Year == 2023 && result.Key.Month == 1 && result.Key.Category == "Food");
//            Assert.NotNull(year2023Month01FoodGroup);
//            Assert.Single(year2023Month01FoodGroup.Transactions);
//            Assert.Contains(year2023Month01FoodGroup.Transactions, t => t.Comment == "Grocery shopping");

//            var year2023Month01TransportGroup = result.FirstOrDefault(result => result.Key.Year == 2023 && result.Key.Month == 1 && result.Key.Category == "Transport");
//            Assert.NotNull(year2023Month01TransportGroup);
//            Assert.Single(year2023Month01TransportGroup.Transactions);
//            Assert.Contains(year2023Month01TransportGroup.Transactions, t => t.Comment == "Bus ticket");

//            var year2024Month03FoodGroup = result.FirstOrDefault(result => result.Key.Year == 2024 && result.Key.Month == 3 && result.Key.Category == "Food");
//            Assert.NotNull(year2024Month03FoodGroup);
//            Assert.Single(year2024Month03FoodGroup.Transactions);
//            Assert.Contains(year2024Month03FoodGroup.Transactions, t => t.Comment == "Dinner");
//        }
//        [Fact]
//        public async Task GetReportByCategoryAndDateAsync_ValidDateRange_ReturnsAllGroupedResults()
//        {
//            // Arrange
//            var dateRange = new ReportQueryObject
//            {
//                StartDate = new DateTimeOffset(2023, 1, 10, 0, 0, 0, TimeSpan.Zero),
//                EndDate = new DateTimeOffset(2025, 1, 6, 0, 0, 0, TimeSpan.Zero)
//            };

//            // Act
//            var result = await _repository.GetReportByCategoryAndDateAsync(dateRange);

//            // Assert
//            Assert.Equal(6, result.Count);

//            var year2023Month01FoodGroup = result.FirstOrDefault(result => result.Key.Year == 2023 && result.Key.Month == 1 && result.Key.Category == "Food");
//            Assert.NotNull(year2023Month01FoodGroup);
//            Assert.Single(year2023Month01FoodGroup.Transactions);
//            Assert.Contains(year2023Month01FoodGroup.Transactions, t => t.Comment == "Grocery shopping");

//            var year2023Month01TransportGroup = result.FirstOrDefault(result => result.Key.Year == 2023 && result.Key.Month == 1 && result.Key.Category == "Transport");
//            Assert.NotNull(year2023Month01TransportGroup);
//            Assert.Single(year2023Month01TransportGroup.Transactions);
//            Assert.Contains(year2023Month01TransportGroup.Transactions, t => t.Comment == "Bus ticket");

//            var year2024Month03FoodGroup = result.FirstOrDefault(result => result.Key.Year == 2024 && result.Key.Month == 3 && result.Key.Category == "Food");
//            Assert.NotNull(year2024Month03FoodGroup);
//            Assert.Single(year2024Month03FoodGroup.Transactions);
//            Assert.Contains(year2024Month03FoodGroup.Transactions, t => t.Comment == "Dinner");

//            var year2024Month03TransportGroup = result.FirstOrDefault(result => result.Key.Year == 2024 && result.Key.Month == 3 && result.Key.Category == "Transport");
//            Assert.NotNull(year2024Month03TransportGroup);
//            Assert.Single(year2024Month03TransportGroup.Transactions);
//            Assert.Contains(year2024Month03TransportGroup.Transactions, t => t.Comment == "Taxi");

//            var year2024Month12EntertainmentGroup = result.FirstOrDefault(result => result.Key.Year == 2024 && result.Key.Month == 12 && result.Key.Category == "Entertainment");
//            Assert.NotNull(year2024Month12EntertainmentGroup);
//            Assert.Single(year2024Month12EntertainmentGroup.Transactions);
//            Assert.Contains(year2024Month12EntertainmentGroup.Transactions, t => t.Comment == "Cinema");

//            var year2025Month01NoCategoryGroup = result.FirstOrDefault(result => result.Key.Year == 2025 && result.Key.Month == 1 && result.Key.Category == "No category");
//            Assert.NotNull(year2025Month01NoCategoryGroup);
//            Assert.Equal(2, year2025Month01NoCategoryGroup.Transactions.Count);
//            Assert.Contains(year2025Month01NoCategoryGroup.Transactions, t => t.Comment == "Unknown expense");
//            Assert.Contains(year2025Month01NoCategoryGroup.Transactions, t => t.Comment == "Misc");
//        }
//        public void Dispose()
//        {
//            _context.Dispose();
//        }
//    }
//}

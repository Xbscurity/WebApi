using api.Models;
using api.Queries;
using api.Specifications;
using api.Tests.Unit.Factories;

namespace api.Tests.Unit.Specifications
{
    public class FinancialTransactionReportSpecificationTests
    {
        private static readonly Guid guid1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid guid2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid guid3 = Guid.Parse("00000000-0000-0000-0000-000000000003");
        private static readonly Guid guid4 = Guid.Parse("00000000-0000-0000-0000-000000000004");
        private static readonly Guid guid5 = Guid.Parse("00000000-0000-0000-0000-000000000005");

        private static readonly DateTimeOffset createdAt1 = new(2026, 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt2 = new(2026, 1, 2, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt3 = new(2026, 1, 3, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt4 = new(2026, 1, 4, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset createdAt5 = new(2026, 1, 5, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly List<FinancialTransaction> _transactions = new()
        {
            FinancialTransactionFactory.Create(id: guid1, userId: "user-1", isActive: true, createdAt: createdAt1),
            FinancialTransactionFactory.Create(id: guid2, userId: "user-1", isActive: false, createdAt: createdAt2),
            FinancialTransactionFactory.Create(id: guid3, userId: "user-1", isActive: true, createdAt: createdAt3),
            FinancialTransactionFactory.Create(id: guid4, userId: "user-1", isActive: true, createdAt: createdAt4),
            FinancialTransactionFactory.Create(id: guid5, userId: "user-2", isActive: true, createdAt: createdAt5),
        };

        [Fact]
        public void UserId_AlwaysApplied_ReturnsAllOwnTransactions()
        {
            // Arrange
            var query = new ReportQuery
            {
                IncludeInactive = true
            };
            var spec = new FinancialTransactionReportSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(t => t.Id).ToList();

            // Assert
            Assert.Equal(_transactions.Count(x => x.AppUserId == "user-1"), result.Count);
            Assert.DoesNotContain(guid5, result);
        }

        [Fact]
        public void UserId_DoesNotMatchAnyTransaction_ReturnsEmpty()
        {
            // Arrange
            var query = new ReportQuery
            {
                IncludeInactive = true
            };
            var spec = new FinancialTransactionReportSpecification(query, "user-999");

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void IncludeInactive_IsFalse_ReturnsOnlyActiveCategoryTransactions()
        {
            // Arrange
            var query = new ReportQuery
            {
                IncludeInactive = false
            };
            var spec = new FinancialTransactionReportSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).ToList();

            // Assert
            Assert.Equal(_transactions.Count(x => x.AppUserId == "user-1" && x.Category.IsActive), result.Count);
            Assert.All(result, r => Assert.True(r.Category.IsActive));
        }

        [Fact]
        public void StartDate_IsProvided_ReturnsTransactionsFromThatDateOnwards()
        {
            // Arrange
            var query = new ReportQuery
            {
                IncludeInactive = true,
                StartDate = createdAt3
            };
            var spec = new FinancialTransactionReportSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(t => t.Id).ToList();

            // Assert
            var expected = new[] { guid3, guid4 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void EndDate_IsProvided_ReturnsTransactionsUpToThatDate()
        {
            // Arrange
            var query = new ReportQuery
            {
                IncludeInactive = true,
                EndDate = createdAt2
            };
            var spec = new FinancialTransactionReportSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(t => t.Id).ToList();

            // Assert
            var expected = new[] { guid1, guid2 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void BothDates_AreProvided_ReturnsTransactionsWithinDateRange()
        {
            // Arrange
            var query = new ReportQuery
            {
                IncludeInactive = true,
                StartDate = createdAt2,
                EndDate = createdAt3
            };
            var spec = new FinancialTransactionReportSpecification(query, "user-1");

            // Act
            var result = spec.Evaluate(_transactions).Select(t => t.Id).ToList();

            // Assert
            var expected = new[] { guid2, guid3 };
            Assert.Equal(expected.OrderBy(x => x), result.OrderBy(x => x));
        }
    }
}
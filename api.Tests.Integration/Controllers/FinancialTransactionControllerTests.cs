using api.Data;
using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Models;
using api.Queries;
using api.Services.Shared;
using api.Tests.Integration.Collections.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace api.Tests.Integration.Controllers
{
    [Collection("IntegrationTestCollection")]
    public class FinancialTransactionControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private static readonly Guid Category1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid Category2Id = Guid.Parse("11111111-1111-1111-1111-222222222222");
        private static readonly Guid Transaction1Id = Guid.Parse("22222222-1111-1111-1111-111111111111");
        private static readonly Guid Transaction2Id = Guid.Parse("22222222-1111-1111-1111-222222222222");
        public FinancialTransactionControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync()
        {
            await _fixture.ResetCheckpointAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task GetAll_WithExistingTransactions_Returns200WithPagedList()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = Category1Id,
                    Name = "Groceries",
                    AppUserId = userId
                });

                db.Transactions.AddRange(
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 100m,
                        AppUserId = userId
                    },
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 200m,
                        AppUserId = userId
                    },
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Income,
                        Amount = 300m,
                        AppUserId = userId
                    }
                );

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<FinancialTransactionOutputDto>>(
                "/api/financial-transactions?page=1&size=2",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Items.Count);
            Assert.Equal(3, response.Pagination.TotalItems);
            Assert.Equal(2, response.Pagination.TotalPages);
            Assert.True(response.Pagination.HasNext);
        }

        [Fact]
        public async Task GetAll_WithDateRangeFilter_ReturnsOnlyTransactionsWithinRange()
        {
            // Arrange
            const string userId1 = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId1);

            _fixture.CurrentTime = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero);
            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category { Id = Category1Id, Name = "User1 Category", AppUserId = userId1 });
                db.Transactions.Add(new FinancialTransaction
                {
                    Id = Transaction1Id,
                    CategoryId = Category1Id,
                    Amount = 100m,
                    Type = FinancialTransactionType.Expense,
                    AppUserId = userId1

                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            _fixture.CurrentTime = _fixture.CurrentTime.AddYears(1);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Transactions.Add(new FinancialTransaction
                {
                    Id = Transaction2Id,
                    CategoryId = Category1Id,
                    Amount = 100m,
                    Type = FinancialTransactionType.Expense,
                    AppUserId = userId1

                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }
            var query = new EntityQuery
            {
                Page = 1,
                Size = 2,
                StartDate = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 5, 1, 1, 1, 1, TimeSpan.Zero)
            };
            // Act
            var response = await client.GetFromJsonAsync<PagedItems<FinancialTransactionOutputDto>>(
                $"/api/financial-transactions?page=1&size=2&startdate={query.StartDate:yyyy-MM-ddTHH:mm:ssZ}" +
                $"&enddate={query.EndDate:yyyy-MM-ddTHH:mm:ssZ}",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(response);

            var ids = response.Items.Select(x => x.Id);
            Assert.Contains(Transaction1Id, ids);
            Assert.DoesNotContain(Transaction2Id, ids);
        }

        [Fact]
        public async Task GetAll_WithMultipleUsers_ReturnsOnlyOwnTransactions()
        {
            // Arrange
            const string userId1 = "user-1";
            const string userId2 = "user-2";
            using var client = await _fixture.CreateUserClientAsync(userId1);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Users.Add(new AppUser { Id = userId2 });

                db.Categories.AddRange(
                    new Category { Id = Category1Id, Name = "User1 Category", AppUserId = userId1 },
                    new Category { Id = Category2Id, Name = "User2 Category", AppUserId = userId2 }
                );

                db.Transactions.AddRange(
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 100m,
                        AppUserId = userId1
                    },
                    new FinancialTransaction
                    {
                        CategoryId = Category2Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 500m,
                        AppUserId = userId2
                    }
                );

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<FinancialTransactionOutputDto>>(
                "/api/financial-transactions?page=1&size=10",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Single(response.Items);
            Assert.Equal(100m, response.Items[0].Amount);
        }

        [Fact]
        public async Task GetAll_WithNoTransactions_Returns200WithEmptyList()
        {
            // Arrange
            using var client = await _fixture.CreateUserClientAsync("user-1");

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<FinancialTransactionOutputDto>>(
                "/api/financial-transactions",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Items);
            Assert.Equal(0, response.Pagination.TotalItems);
        }

        [Fact]
        public async Task GetAll_WhenQueryIsInvalid_Returns400BadRequest()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            // Act
            var response = await client.GetAsync(
                "/api/financial-transactions?Page=-1",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_WhenSortByIsInvalid_Returns422UnprocessableEntity()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            // Act
            var response = await client.GetAsync(
                "/api/financial-transactions?sortby=invalid",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithExistingTransaction_Returns200WithTransaction()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = Category1Id,
                    Name = "Groceries",
                    AppUserId = userId
                });

                db.Transactions.Add(new FinancialTransaction
                {
                    Id = Transaction1Id,
                    CategoryId = Category1Id,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = userId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<FinancialTransactionOutputDto>(
                $"/api/financial-transactions/{Transaction1Id}",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(Transaction1Id, response.Id);
            Assert.Equal(100m, response.Amount);
        }

        [Fact]
        public async Task GetById_WhenTransactionDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync(
                $"/api/financial-transactions/{nonExistentId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenTransactionBelongsToAnotherUser_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            const string otherUserId = "user-2";

            using var client = await _fixture.CreateUserClientAsync(ownerId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Users.Add(new AppUser { Id = otherUserId });

                db.Categories.Add(new Category
                {
                    Id = Category1Id,
                    Name = "Other's Category",
                    AppUserId = otherUserId
                });

                db.Transactions.Add(new FinancialTransaction
                {
                    Id = Transaction1Id,
                    CategoryId = Category1Id,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = otherUserId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetAsync(
                $"/api/financial-transactions/{Transaction1Id}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithValidData_Returns201WithTransaction()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = Category1Id,
                    Name = "Groceries",
                    AppUserId = userId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionCreateInputDto
            {
                CategoryId = Category1Id,
                Type = FinancialTransactionType.Expense,
                Amount = 150m,
                Comment = "Test"
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/financial-transactions",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<FinancialTransactionOutputDto>(
                _fixture.JsonOptions, TestContext.Current.CancellationToken);
            Assert.NotNull(created);
            Assert.Equal(150m, created.Amount);
            Assert.NotEqual(Guid.Empty, created.Id);

            Assert.NotNull(response.Headers.Location);
            Assert.Contains(created.Id.ToString(), response.Headers.Location!.ToString());

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var saved = await db2.Transactions.FindAsync(
                [created.Id], TestContext.Current.CancellationToken);
            Assert.NotNull(saved);
            Assert.Equal(userId, saved.AppUserId);
        }

        [Fact]
        public async Task Create_WhenAmountIsInvalid_Returns400BadRequest()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = userId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionCreateInputDto
            {
                CategoryId = categoryId,
                Type = FinancialTransactionType.Expense,
                Amount = -10,
                Comment = "Test"
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/financial-transactions",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var nonExistentCategoryId = Guid.NewGuid();

            var dto = new FinancialTransactionCreateInputDto
            {
                CategoryId = nonExistentCategoryId,
                Type = FinancialTransactionType.Expense,
                Amount = 100m,
                Comment = "Test"
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/financial-transactions",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_WhenCategoryBelongsToAnotherUser_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            const string otherUserId = "user-2";
            using var client = await _fixture.CreateUserClientAsync(ownerId);
            var otherUsersCategoryId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Users.Add(new AppUser { Id = otherUserId });

                db.Categories.Add(new Category
                {
                    Id = otherUsersCategoryId,
                    Name = "Other's Category",
                    AppUserId = otherUserId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionCreateInputDto
            {
                CategoryId = otherUsersCategoryId,
                Type = FinancialTransactionType.Expense,
                Amount = 100m,
                Comment = "Test"
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/financial-transactions",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var exists = await db2.Transactions.AnyAsync(
                t => t.CategoryId == otherUsersCategoryId,
                TestContext.Current.CancellationToken);
            Assert.False(exists);
        }

        [Fact]
        public async Task Update_WithValidData_Returns200WithUpdatedTransaction()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Groceries",
                    AppUserId = userId
                });

                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = userId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionUpdateInputDto
            {
                CategoryId = categoryId,
                Type = FinancialTransactionType.Expense,
                Amount = 250m,
                Comment = "Updated"
            };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/financial-transactions/{transactionId}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<FinancialTransactionOutputDto>(
                _fixture.JsonOptions, TestContext.Current.CancellationToken);
            Assert.NotNull(updated);
            Assert.Equal(250m, updated.Amount);
            Assert.Equal("Updated", updated.Comment);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var saved = await db2.Transactions.FindAsync(
                [transactionId], TestContext.Current.CancellationToken);
            Assert.NotNull(saved);
            Assert.Equal(250m, saved.Amount);
        }

        [Fact]
        public async Task Update_WhenInputIsInvalid_Returns400BadRequest()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = userId });
                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = userId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionUpdateInputDto
            {
                CategoryId = categoryId,
                Type = FinancialTransactionType.Expense,
                Amount = -50m,
                Comment = "Test"
            };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/financial-transactions/{transactionId}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_WhenTransactionDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var nonExistentId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category { Id = Category1Id, Name = "Groceries", AppUserId = userId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionUpdateInputDto
            {
                CategoryId = Category1Id,
                Type = FinancialTransactionType.Expense,
                Amount = 100m,
                Comment = "Test"
            };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/financial-transactions/{nonExistentId}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WhenTransactionBelongsToAnotherUser_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            const string otherUserId = "user-2";
            var categoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();

            using var client = await _fixture.CreateUserClientAsync(ownerId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Users.Add(new AppUser { Id = otherUserId });

                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Other's Category",
                    AppUserId = otherUserId
                });

                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = otherUserId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionUpdateInputDto
            {
                CategoryId = categoryId,
                Type = FinancialTransactionType.Expense,
                Amount = 999m,
                Comment = "Hacked"
            };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/financial-transactions/{transactionId}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var untouched = await db2.Transactions.FindAsync(
                [transactionId], TestContext.Current.CancellationToken);
            Assert.NotNull(untouched);
            Assert.Equal(100m, untouched.Amount);
        }

        [Fact]
        public async Task Update_WhenNewCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var transactionId = Guid.NewGuid();
            var nonExistentCategoryId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category { Id = Category1Id, Name = "Groceries", AppUserId = userId });
                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = Category1Id,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = userId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionUpdateInputDto
            {
                CategoryId = nonExistentCategoryId,
                Type = FinancialTransactionType.Expense,
                Amount = 100m,
                Comment = "Test"
            };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/financial-transactions/{transactionId}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WhenNewCategoryBelongsToAnotherUser_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            const string otherUserId = "user-2";
            using var client = await _fixture.CreateUserClientAsync(ownerId);
            var otherUsersCategoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Users.Add(new AppUser { Id = otherUserId });

                db.Categories.AddRange(
                    new Category { Id = Category1Id, Name = "Own Category", AppUserId = ownerId },
                    new Category { Id = otherUsersCategoryId, Name = "Other's Category", AppUserId = otherUserId }
                );

                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = Category1Id,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = ownerId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionUpdateInputDto
            {
                CategoryId = otherUsersCategoryId,
                Type = FinancialTransactionType.Expense,
                Amount = 100m,
                Comment = "Test"
            };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/financial-transactions/{transactionId}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var untouched = await db2.Transactions.FindAsync(
                [transactionId], TestContext.Current.CancellationToken);
            Assert.NotNull(untouched);
            Assert.Equal(Category1Id, untouched.CategoryId);
        }

        [Fact]
        public async Task Delete_WithExistingTransaction_Returns204NoContent()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = userId });
                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = userId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.DeleteAsync(
                $"/api/financial-transactions/{transactionId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deleted = await db2.Transactions.FindAsync(
                [transactionId], TestContext.Current.CancellationToken);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task Delete_WhenTransactionDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.DeleteAsync(
                $"/api/financial-transactions/{nonExistentId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WhenTransactionBelongsToAnotherUser_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            const string otherUserId = "user-2";
            var categoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();

            using var client = await _fixture.CreateUserClientAsync(ownerId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Users.Add(new AppUser { Id = otherUserId });

                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Other's Category",
                    AppUserId = otherUserId
                });

                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = otherUserId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.DeleteAsync(
                $"/api/financial-transactions/{transactionId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var untouched = await db2.Transactions.FindAsync(
                [transactionId], TestContext.Current.CancellationToken);
            Assert.NotNull(untouched);
        }

        [Fact]
        public async Task GetReport_ByCategory_Returns200WithGroupedByCategoryReport()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.AddRange(
                    new Category { Id = Category1Id, Name = "Groceries", AppUserId = userId },
                    new Category { Id = Category2Id, Name = "Transport", AppUserId = userId }
                );

                db.Transactions.AddRange(
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 100m,
                        AppUserId = userId
                    },
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 50m,
                        AppUserId = userId
                    },
                    new FinancialTransaction
                    {
                        CategoryId = Category2Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 30m,
                        AppUserId = userId
                    }
                );

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<GroupedReportOutputDto>>(
                "/api/financial-transactions/report?page=1&size=10&key=bycategory",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Items.Count);
            Assert.Equal(2, response.Pagination.TotalItems);

            var groceriesGroup = response.Items.Single(g => g.Transactions.Any(t => t.CategoryId == Category1Id));
            Assert.Equal(2, groceriesGroup.Count);
            Assert.Equal(-150m, groceriesGroup.TotalAmount);

            var transportGroup = response.Items.Single(g => g.Transactions.Any(t => t.CategoryId == Category2Id));
            Assert.Equal(1, transportGroup.Count);
            Assert.Equal(-30m, transportGroup.TotalAmount);
        }

        [Fact]
        public async Task GetReport_ByDate_Returns200WithGroupedByDateReport()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            _fixture.CurrentTime = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero);
            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(
                    new Category { Id = Category1Id, Name = "Groceries", AppUserId = userId }
                );

                db.Transactions.AddRange(
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 100m,
                        AppUserId = userId
                    },
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 50m,
                        AppUserId = userId
                    });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            _fixture.CurrentTime = _fixture.CurrentTime.AddMonths(1);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Transactions.AddRange(
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 72m,
                        AppUserId = userId
                    },
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 55m,
                        AppUserId = userId
                    });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<GroupedReportOutputDto>>(
                "/api/financial-transactions/report?page=1&size=10&key=bydate",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Items.Count);
            Assert.Equal(2, response.Pagination.TotalItems);

            var januaryGroup = Assert.Single(
                response.Items,
                g => g.GroupKey is ReportKey.DateKey dk && dk is { Year: 2026, Month: 1 });
            Assert.Equal(2, januaryGroup.Count);
            Assert.Equal(-150m, januaryGroup.TotalAmount);

            var februaryGroup = Assert.Single(
                response.Items,
                g => g.GroupKey is ReportKey.DateKey dk && dk is { Year: 2026, Month: 2 });
            Assert.Equal(2, februaryGroup.Count);
            Assert.Equal(-127m, februaryGroup.TotalAmount);
        }

        [Fact]
        public async Task GetReport_ByCategoryAndDate_Returns200WithGroupedByCategoryAndDateReport()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            _fixture.CurrentTime = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero);
            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.AddRange(
                    new Category { Id = Category1Id, Name = "Groceries", AppUserId = userId },
                    new Category { Id = Category2Id, Name = "Transport", AppUserId = userId }
                );
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);

                db.Transactions.AddRange(
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 100m,
                        AppUserId = userId
                    },
                    new FinancialTransaction
                    {
                        CategoryId = Category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 50m,
                        AppUserId = userId
                    },
                    new FinancialTransaction
                    {
                        CategoryId = Category2Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 30m,
                        AppUserId = userId
                    });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            _fixture.CurrentTime = _fixture.CurrentTime.AddMonths(1);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Transactions.Add(new FinancialTransaction
                {
                    CategoryId = Category1Id,
                    Type = FinancialTransactionType.Expense,
                    Amount = 200m,
                    AppUserId = userId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<GroupedReportOutputDto>>(
                "/api/financial-transactions/report?page=1&size=10&key=bycategoryanddate",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(3, response.Items.Count);
            Assert.Equal(3, response.Pagination.TotalItems);

            var groceriesJanuary = Assert.Single(
                response.Items,
                g => g.GroupKey is ReportKey.CategoryAndDateKey k
                     && k is { Name: "Groceries", Year: 2026, Month: 1 });
            Assert.Equal(2, groceriesJanuary.Count);
            Assert.Equal(-150m, groceriesJanuary.TotalAmount);

            var transportJanuary = Assert.Single(
                response.Items,
                g => g.GroupKey is ReportKey.CategoryAndDateKey k
                     && k is { Name: "Transport", Year: 2026, Month: 1 });
            Assert.Equal(1, transportJanuary.Count);
            Assert.Equal(-30m, transportJanuary.TotalAmount);

            var groceriesFebruary = Assert.Single(
                response.Items,
                g => g.GroupKey is ReportKey.CategoryAndDateKey k
                     && k is { Name: "Groceries", Year: 2026, Month: 2 });
            Assert.Equal(1, groceriesFebruary.Count);
            Assert.Equal(-200m, groceriesFebruary.TotalAmount);
        }

        [Fact]
        public async Task GetReport_WithDateRangeFilter_ReturnsOnlyTransactionsWithinRange()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();

            var startDate = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero);
            var endDate = new DateTimeOffset(2026, 7, 1, 1, 1, 1, TimeSpan.Zero);

            _fixture.CurrentTime = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {

                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = userId });

                db.Transactions.Add(
                    new FinancialTransaction
                    {
                        Id = Transaction1Id,
                        CategoryId = categoryId,
                        Type = FinancialTransactionType.Expense,
                        Amount = 100m,
                        AppUserId = userId,
                    });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            _fixture.CurrentTime = _fixture.CurrentTime.AddYears(1);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Transactions.Add(
                new FinancialTransaction
                {
                    Id = Transaction2Id,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 500m,
                    AppUserId = userId,
                }
                );

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<GroupedReportOutputDto>>(
                $"/api/financial-transactions/report?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}" +
                $"&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            var allTransactionIds = response.Items.SelectMany(g => g.Transactions).Select(t => t.Id).ToList();
            Assert.Contains(Transaction1Id, allTransactionIds);
            Assert.DoesNotContain(Transaction2Id, allTransactionIds);
        }

        [Fact]
        public async Task GetReport_WithMultipleUsers_ReturnsOnlyOwnTransactions()
        {
            // Arrange
            const string userId1 = "user-1";
            const string userId2 = "user-2";
            using var client = await _fixture.CreateUserClientAsync(userId1);
            var category1Id = Guid.NewGuid();
            var category2Id = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Users.Add(new AppUser { Id = userId2 });

                db.Categories.AddRange(
                    new Category { Id = category1Id, Name = "User1 Category", AppUserId = userId1 },
                    new Category { Id = category2Id, Name = "User2 Category", AppUserId = userId2 }
                );

                db.Transactions.AddRange(
                    new FinancialTransaction
                    {
                        CategoryId = category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 100m,
                        AppUserId = userId1
                    },
                    new FinancialTransaction
                    {
                        CategoryId = category2Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 500m,
                        AppUserId = userId2
                    }
                );

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<GroupedReportOutputDto>>(
                "/api/financial-transactions/report?page=1&size=10",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Single(response.Items);
            Assert.All(
                response.Items.SelectMany(g => g.Transactions),
                t => Assert.Equal(category1Id, t.CategoryId));
        }

        [Fact]
        public async Task GetReport_WithNoTransactions_Returns200WithEmptyReport()
        {
            // Arrange
            using var client = await _fixture.CreateUserClientAsync("user-1");

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<GroupedReportOutputDto>>(
                "/api/financial-transactions/report",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Items);
            Assert.Equal(0, response.Pagination.TotalItems);
        }

        [Fact]
        public async Task GetReport_WhenQueryIsInvalid_Returns400BadRequest()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            // Act
            var response = await client.GetAsync(
                "/api/financial-transactions/report?Page=-1",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
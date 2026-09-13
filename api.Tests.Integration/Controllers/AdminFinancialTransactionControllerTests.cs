using api.Data;
using api.Dtos.FinancialTransaction;
using api.Enums;
using api.Models;
using api.Services.Shared;
using api.Tests.Integration.Collections.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace api.Tests.Integration.Controllers
{
    [Collection("IntegrationTestCollection")]
    public class AdminFinancialTransactionControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;

        public AdminFinancialTransactionControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync()
        {
            await _fixture.ResetCheckpointAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task GetAll_WithExistingTransactions_Returns200WithAllUsersTransactions()
        {
            // Arrange
            const string user1Id = "user-1";
            const string user2Id = "user-2";
            var category1Id = Guid.NewGuid();
            var category2Id = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.AddRange(new AppUser { Id = user1Id }, new AppUser { Id = user2Id });
                db.Categories.AddRange(
                    new Category { Id = category1Id, Name = "User1 Category", AppUserId = user1Id },
                    new Category { Id = category2Id, Name = "User2 Category", AppUserId = user2Id });

                db.Transactions.AddRange(
                    new FinancialTransaction
                    {
                        CategoryId = category1Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 100m,
                        Comment = "Test",
                        AppUserId = user1Id
                    },
                    new FinancialTransaction
                    {
                        CategoryId = category2Id,
                        Type = FinancialTransactionType.Expense,
                        Amount = 200m,
                        Comment = "Test",
                        AppUserId = user2Id
                    });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<AdminFinancialTransactionOutputDto>>(
                "/api/admin/financial-transactions?page=1&size=10",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Items.Count);
        }

        [Fact]
        public async Task GetAll_WithNoTransactions_Returns200WithEmptyList()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<AdminFinancialTransactionOutputDto>>(
                "/api/admin/financial-transactions",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Items);
            Assert.Equal(0, response.Pagination.TotalItems);
        }

        [Fact]
        public async Task GetAll_WhenSortByIsInvalid_Returns422UnprocessableEntity()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.GetAsync(
                "/api/admin/financial-transactions?sortby=invalid",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithAnyUsersTransaction_Returns200WithTransaction()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = ownerId });
                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    Comment = "Test",
                    AppUserId = ownerId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<AdminFinancialTransactionOutputDto>(
                $"/api/admin/financial-transactions/{transactionId}",
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(transactionId, response.Id);
        }

        [Fact]
        public async Task GetById_WhenTransactionDoesNotExist_Returns404NotFound()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.GetAsync(
                $"/api/admin/financial-transactions/{Guid.NewGuid()}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenAnonymous_Returns401Unauthorized()
        {
            // Arrange
            using var client = _fixture.CreateAnonymousClient();

            // Act
            var response = await client.GetAsync(
                $"/api/admin/financial-transactions/{Guid.NewGuid()}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenUserIsNotAdmin_Returns403Forbidden()
        {
            // Arrange
            using var client = await _fixture.CreateUserClientAsync("user-1");

            // Act
            var response = await client.GetAsync(
                $"/api/admin/financial-transactions/{Guid.NewGuid()}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithValidData_Returns201WithTransactionOwnedByCategoryUser()
        {
            // Arrange
            const string categoryOwnerId = "user-1";
            var categoryId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = categoryOwnerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = categoryOwnerId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new AdminFinancialTransactionCreateInputDto
            {
                CategoryId = categoryId,
                Type = FinancialTransactionType.Expense,
                Amount = 150m,
                Comment = "Test"
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/admin/financial-transactions",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<AdminFinancialTransactionOutputDto>(
                _fixture.JsonOptions, TestContext.Current.CancellationToken);
            Assert.NotNull(created);
            Assert.Equal(150m, created.Amount);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var saved = await db2.Transactions.FindAsync(
                [created.Id], TestContext.Current.CancellationToken);
            Assert.NotNull(saved);
            Assert.Equal(categoryOwnerId, saved.AppUserId);
        }

        [Fact]
        public async Task Create_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");
            var dto = new AdminFinancialTransactionCreateInputDto
            {
                CategoryId = Guid.NewGuid(),
                Type = FinancialTransactionType.Expense,
                Amount = 100m,
                Comment = "Test"
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/admin/financial-transactions",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_WhenAmountIsInvalid_Returns400BadRequest()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            const string ownerId = "user-1";
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = ownerId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new AdminFinancialTransactionCreateInputDto
            {
                CategoryId = categoryId,
                Type = FinancialTransactionType.Expense,
                Amount = 0,
                Comment = "Test"
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/admin/financial-transactions",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithValidData_Returns200WithUpdatedTransaction()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = ownerId });
                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    Comment = "Test",
                    AppUserId = ownerId
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
                $"/api/admin/financial-transactions/{transactionId}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<AdminFinancialTransactionOutputDto>(
                _fixture.JsonOptions, TestContext.Current.CancellationToken);
            Assert.NotNull(updated);
            Assert.Equal(250m, updated.Amount);
        }

        [Fact]
        public async Task Update_WhenTransactionDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var dto = new FinancialTransactionUpdateInputDto
            {
                CategoryId = Guid.NewGuid(),
                Type = FinancialTransactionType.Expense,
                Amount = 100m,
                Comment = "Test"
            };
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/admin/financial-transactions/{Guid.NewGuid()}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WhenNewCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = ownerId });
                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    Comment = "Test",
                    AppUserId = ownerId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new FinancialTransactionUpdateInputDto
            {
                CategoryId = Guid.NewGuid(),
                Type = FinancialTransactionType.Expense,
                Amount = 100m,
                Comment = "Test"
            };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/admin/financial-transactions/{transactionId}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WhenCategoryBelongsToAnotherUser_Returns422UnprocessableEntity()
        {
            // Arrange
            const string transactionOwnerId = "user-1";
            const string otherUserId = "user-2";
            var ownCategoryId = Guid.NewGuid();
            var otherUsersCategoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.AddRange(
                    new AppUser { Id = transactionOwnerId },
                    new AppUser { Id = otherUserId });

                db.Categories.AddRange(
                    new Category { Id = ownCategoryId, Name = "Own Category", AppUserId = transactionOwnerId },
                    new Category { Id = otherUsersCategoryId, Name = "Other's Category", AppUserId = otherUserId });

                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = ownCategoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    Comment = "Test",
                    AppUserId = transactionOwnerId
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
                $"/api/admin/financial-transactions/{transactionId}",
                dto,
                _fixture.JsonOptions,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var untouched = await db2.Transactions.FindAsync(
                [transactionId], TestContext.Current.CancellationToken);
            Assert.NotNull(untouched);
            Assert.Equal(ownCategoryId, untouched.CategoryId);

        }
        [Fact]
        public async Task Delete_WithExistingTransaction_Returns204NoContent()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Groceries", AppUserId = ownerId });
                db.Transactions.Add(new FinancialTransaction
                {
                    Id = transactionId,
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    Comment = "Test",
                    AppUserId = ownerId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.DeleteAsync(
                $"/api/admin/financial-transactions/{transactionId}",
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
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.DeleteAsync(
                $"/api/admin/financial-transactions/{Guid.NewGuid()}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }


}

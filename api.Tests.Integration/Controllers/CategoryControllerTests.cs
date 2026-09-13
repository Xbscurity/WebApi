using api.Data;
using api.Dtos.Category;
using api.Enums;
using api.Models;
using api.Services.Shared;
using api.Tests.Integration.Collections.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace api.Tests.Integration.Controllers
{
    [Collection("IntegrationTestCollection")]
    public class CategoryControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private static readonly Guid Category1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid Category2Id = Guid.Parse("11111111-1111-1111-1111-222222222222");

        public CategoryControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync()
        {
            await _fixture.ResetCheckpointAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task GetAll_WithExistingCategories_Returns200WithPagedList()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.AddRange(
                    new Category
                    {
                        Name = "Books",
                        AppUserId = userId
                    },

                    new Category
                    {
                        Name = "Clothes",
                        AppUserId = userId
                    },

                    new Category
                    {
                        Name = "Food",
                        AppUserId = userId
                    }
            );

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<CategoryOutputDto>>(
                "/api/categories?page=1&size=2",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Items.Count);
            Assert.Equal(3, response.Pagination.TotalItems);
            Assert.Equal(2, response.Pagination.TotalPages);
            Assert.True(response.Pagination.HasNext);
        }

        [Fact]
        public async Task GetAll_WithDateRangeFilter_ReturnsOnlyCategoriesWithinRange()
        {
            // Arrange
            const string userId1 = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId1);
            _fixture.CurrentTime = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero);
            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = Category1Id,
                    Name = "Test1",
                    AppUserId = userId1
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }
            _fixture.CurrentTime = _fixture.CurrentTime.AddYears(1);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = Category2Id,
                    Name = "Test2",
                    AppUserId = userId1
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var startDate = new DateTimeOffset(2026, 1, 1, 1, 1, 1, TimeSpan.Zero);
            var endDate = new DateTimeOffset(2026, 7, 1, 1, 1, 1, TimeSpan.Zero);

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<CategoryOutputDto>>(
                $"/api/categories?page=1&size=10&startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}" +
                $"&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            var ids = response.Items.Select(x => x.Id).ToList();
            Assert.Contains(Category1Id, ids);
            Assert.DoesNotContain(Category2Id, ids);
        }

        [Fact]
        public async Task GetAll_WithMultipleUsers_ReturnsOnlyOwnCategories()
        {
            // Arrange
            const string userId1 = "user-1";
            const string userId2 = "user-2";
            using var client = await _fixture.CreateUserClientAsync(userId1);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.AddRange(
                    new Category
                    {
                        Name = "User1 Category",
                        AppUserId = userId1
                    },
                    new Category
                    {
                        Name = "User2 Category",
                        AppUserId = userId2
                    }
                );
                db.Users.Add(
                    new AppUser
                    {
                        Id = userId2,
                    }

                    );

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<CategoryOutputDto>>(
                "/api/categories?page=1&size=10",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Single(response.Items);
            Assert.Equal("User1 Category", response.Items[0].Name);
        }

        [Fact]
        public async Task GetAll_WithNoCategories_Returns200WithEmptyList()
        {
            using var client = await _fixture.CreateUserClientAsync("user-1");

            var response = await client.GetFromJsonAsync<PagedItems<CategoryOutputDto>>(
                "/api/categories",
                TestContext.Current.CancellationToken);

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
                "/api/categories?Page=-1",
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
                "/api/categories?sortby=invalid",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithExistingCategory_Returns200WithCategory()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Books",
                    AppUserId = userId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<CategoryOutputDto>(
                $"/api/categories/{categoryId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(categoryId, response.Id);
            Assert.Equal("Books", response.Name);
        }

        [Fact]
        public async Task GetById_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync(
                $"/api/categories/{nonExistentId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenCategoryBelongsToAnotherUser_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            const string otherUserId = "user-2";
            var categoryId = Guid.NewGuid();

            using var client = await _fixture.CreateUserClientAsync(ownerId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Owner's Category",
                    AppUserId = otherUserId
                });

                db.Users.Add(new AppUser
                {
                    Id = otherUserId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetAsync(
                $"/api/categories/{categoryId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithValidData_Returns201WithCategory()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var dto = new CategoryCreateInputDto { Name = "Books" };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/categories",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<CategoryOutputDto>(
                TestContext.Current.CancellationToken);
            Assert.NotNull(created);
            Assert.Equal("Books", created.Name);
            Assert.NotEqual(Guid.Empty, created.Id);

            Assert.NotNull(response.Headers.Location);
            Assert.Contains(created.Id.ToString(), response.Headers.Location!.ToString());

            await using var scope = _fixture.Factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedCategory = await db.Categories.FindAsync(
                [created.Id], TestContext.Current.CancellationToken);

            Assert.NotNull(savedCategory);
            Assert.Equal(userId, savedCategory.AppUserId);
        }

        [Fact]
        public async Task Create_WhenNameIsInvalid_Returns400BadRequest()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var dto = new CategoryCreateInputDto { Name = "" };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/categories",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithValidData_Returns200WithUpdatedCategory()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Books",
                    AppUserId = userId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new CategoryUpdateInputDto { Name = "Updated Books" };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/categories/{categoryId}",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<CategoryOutputDto>(
                TestContext.Current.CancellationToken);
            Assert.NotNull(updated);
            Assert.Equal("Updated Books", updated.Name);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedCategory = await db2.Categories.FindAsync(
                [categoryId], TestContext.Current.CancellationToken);
            Assert.NotNull(savedCategory);
            Assert.Equal("Updated Books", savedCategory.Name);
        }

        [Fact]
        public async Task Update_WhenNameIsInvalid_Returns400BadRequest()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Books",
                    AppUserId = userId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new CategoryUpdateInputDto { Name = "" };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/categories/{categoryId}",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var nonExistentId = Guid.NewGuid();
            var dto = new CategoryUpdateInputDto { Name = "Books" };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/categories/{nonExistentId}",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WhenCategoryBelongsToAnotherUser_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            const string otherUserId = "user-2";
            var categoryId = Guid.NewGuid();

            using var client = await _fixture.CreateUserClientAsync(ownerId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Other's Category",
                    AppUserId = otherUserId
                });

                db.Users.Add(new AppUser { Id = otherUserId });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new CategoryUpdateInputDto { Name = "Hacked Name" };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/categories/{categoryId}",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var untouchedCategory = await db2.Categories.FindAsync(
                [categoryId], TestContext.Current.CancellationToken);
            Assert.NotNull(untouchedCategory);
            Assert.Equal("Other's Category", untouchedCategory.Name);
        }

        [Fact]
        public async Task SetActive_WithValidData_Returns200AndTogglesStatus()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();
            var input = new SetActiveInputDto
            {
                IsActive = false,
            };
            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Books",
                    IsActive = true,
                    AppUserId = userId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.PatchAsJsonAsync(
                $"/api/categories/{categoryId}/active",
                input,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<SetActiveOutputDto>(
                TestContext.Current.CancellationToken);
            Assert.NotNull(result);
            Assert.False(result.IsActive);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedCategory = await db2.Categories.FindAsync(
                [categoryId], TestContext.Current.CancellationToken);
            Assert.NotNull(savedCategory);
            Assert.False(savedCategory.IsActive);
        }

        [Fact]
        public async Task SetActive_WhenIsActiveIsMissing_Returns400BadRequest()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();
            var emptyBody = new StringContent("{}", Encoding.UTF8, "application/json");

            // Act
            var response = await client.PatchAsync(
                $"/api/categories/{categoryId}/active",
                emptyBody,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SetActive_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var nonExistentId = Guid.NewGuid();
            var input = new SetActiveInputDto
            {
                IsActive = false
            };
            // Act
            var response = await client.PatchAsJsonAsync(
                $"/api/categories/{nonExistentId}/active",
                input,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SetActive_WhenCategoryBelongsToAnotherUser_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            const string otherUserId = "user-2";
            var categoryId = Guid.NewGuid();

            using var client = await _fixture.CreateUserClientAsync(ownerId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Other's Category",
                    IsActive = true,
                    AppUserId = otherUserId
                });

                db.Users.Add(new AppUser { Id = otherUserId });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var input = new SetActiveInputDto
            {
                IsActive = false
            };

            // Act
            var response = await client.PatchAsJsonAsync(
                $"/api/categories/{categoryId}/active",
                input,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var untouchedCategory = await db2.Categories.FindAsync(
                [categoryId], TestContext.Current.CancellationToken);
            Assert.NotNull(untouchedCategory);
            Assert.True(untouchedCategory.IsActive);
        }

        [Fact]
        public async Task Delete_WithExistingCategory_Returns204NoContent()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Books",
                    IsActive = true,
                    AppUserId = userId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.DeleteAsync(
                $"/api/categories/{categoryId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedCategory = await db2.Categories.FindAsync(
                [categoryId], TestContext.Current.CancellationToken);
            Assert.Null(deletedCategory);
        }

        [Fact]
        public async Task Delete_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.DeleteAsync(
                $"/api/categories/{nonExistentId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WhenCategoryBelongsToAnotherUser_Returns404NotFound()
        {
            // Arrange
            const string ownerId = "user-1";
            const string otherUserId = "user-2";
            var categoryId = Guid.NewGuid();

            using var client = await _fixture.CreateUserClientAsync(ownerId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Other's Category",
                    IsActive = true,
                    AppUserId = otherUserId
                });

                db.Users.Add(new AppUser { Id = otherUserId });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.DeleteAsync(
                $"/api/categories/{categoryId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var untouchedCategory = await db2.Categories.FindAsync(
                [categoryId], TestContext.Current.CancellationToken);
            Assert.NotNull(untouchedCategory);
        }

        [Fact]
        public async Task Delete_WhenCategoryHasReferencedTransactions_Returns409Conflict()
        {
            // Arrange
            const string userId = "user-1";
            using var client = await _fixture.CreateUserClientAsync(userId);
            var categoryId = Guid.NewGuid();

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Categories.Add(new Category
                {
                    Id = categoryId,
                    Name = "Books",
                    IsActive = true,
                    AppUserId = userId
                });

                db.Transactions.Add(new FinancialTransaction
                {
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = userId
                });

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.DeleteAsync(
                $"/api/categories/{categoryId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var untouchedCategory = await db2.Categories.FindAsync(
                [categoryId], TestContext.Current.CancellationToken);
            Assert.NotNull(untouchedCategory);
        }
    }
}
using api.Data;
using api.Dtos.Category;
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
    public class AdminCategoryControllerTests : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;
        private static readonly Guid Category1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid Category2Id = Guid.Parse("11111111-1111-1111-1111-222222222222");

        public AdminCategoryControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync()
        {
            await _fixture.ResetCheckpointAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task GetAll_WithExistingCategories_Returns200WithAllUsersCategories()
        {
            // Arrange
            const string adminId = "admin-1";
            const string user1Id = "user-1";
            const string user2Id = "user-2";
            using var client = await _fixture.CreateAdminClientAsync(adminId);

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.AddRange(
                    new AppUser { Id = user1Id },
                    new AppUser { Id = user2Id });

                db.Categories.AddRange(
                    new Category { Id = Category1Id, Name = "User1 Category", AppUserId = user1Id },
                    new Category { Id = Category2Id, Name = "User2 Category", AppUserId = user2Id }
                );

                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<AdminCategoryOutputDto>>(
                "/api/admin/categories?page=1&size=10",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Items.Count);
        }

        [Fact]
        public async Task GetAll_WithNoCategories_Returns200WithEmptyList()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.GetFromJsonAsync<PagedItems<AdminCategoryOutputDto>>(
                "/api/admin/categories",
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
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.GetAsync(
                "/api/admin/categories?page=-1",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_WhenSortIsInvalid_Returns422UnprocessableEntity()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.GetAsync(
                "/api/admin/categories?sortby=invalid",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithAnyUsersCategory_Returns200WithCategory()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Books", AppUserId = ownerId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetFromJsonAsync<AdminCategoryOutputDto>(
                $"/api/admin/categories/{categoryId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(categoryId, response.Id);
            Assert.Equal(ownerId, response.AppUserId);
        }

        [Fact]
        public async Task GetById_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.GetAsync(
                $"/api/admin/categories/{Guid.NewGuid()}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithValidData_Returns201WithCategoryForTargetUser()
        {
            // Arrange
            const string targetUserId = "user-1";
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = targetUserId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new AdminCategoryCreateInputDto { Name = "Books", AppUserId = targetUserId };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/admin/categories",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<AdminCategoryOutputDto>(
                TestContext.Current.CancellationToken);
            Assert.NotNull(created);
            Assert.Equal(targetUserId, created.AppUserId);

            Assert.NotNull(response.Headers.Location);
            Assert.Contains(created.Id.ToString(), response.Headers.Location!.ToString());
        }

        [Fact]
        public async Task Create_WhenNameIsInvalid_Returns400BadRequest()
        {
            // Arrange
            const string targetUserId = "user-1";
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = targetUserId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new AdminCategoryCreateInputDto { Name = "", AppUserId = targetUserId }; ;

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/admin/categories",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WhenTargetUserDoesNotExist_Returns404NotFound()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");
            var dto = new AdminCategoryCreateInputDto
            {
                Name = "Books",
                AppUserId = "non-existent-user"
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/admin/categories",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithAnyUsersCategory_Returns200WithUpdatedCategory()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Books", AppUserId = ownerId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new CategoryUpdateInputDto { Name = "Updated Books" };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/admin/categories/{categoryId}",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<AdminCategoryOutputDto>(
                TestContext.Current.CancellationToken);
            Assert.NotNull(updated);
            Assert.Equal("Updated Books", updated.Name);
            Assert.Equal(ownerId, updated.AppUserId);
        }

        [Fact]
        public async Task Update_WhenNameIsInvalid_Returns400BadRequest()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Books", AppUserId = ownerId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var dto = new CategoryUpdateInputDto { Name = "" };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/admin/categories/{categoryId}",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");
            var dto = new CategoryUpdateInputDto { Name = "Books" };

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/admin/categories/{Guid.NewGuid()}",
                dto,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SetActive_WithAnyUsersCategory_Returns200AndTogglesStatus()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Books", IsActive = true, AppUserId = ownerId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }
            var input = new SetActiveInputDto
            {
                IsActive = false
            };

            // Act
            var response = await client.PatchAsJsonAsync(
                $"/api/admin/categories/{categoryId}/active?isActive=false",
                input,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<SetActiveOutputDto>(
                TestContext.Current.CancellationToken);
            Assert.NotNull(result);
            Assert.False(result.IsActive);
        }

        [Fact]
        public async Task SetActive_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            var input = new SetActiveInputDto
            {
                IsActive = true
            };

            // Act
            var response = await client.PatchAsJsonAsync(
                $"/api/admin/categories/{Guid.NewGuid()}/active?isActive=false",
                input,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithAnyUsersCategory_Returns204NoContent()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Books", AppUserId = ownerId });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.DeleteAsync(
                $"/api/admin/categories/{categoryId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            await using var scope2 = _fixture.Factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deleted = await db2.Categories.FindAsync(
                [categoryId], TestContext.Current.CancellationToken);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task Delete_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            // Act
            var response = await client.DeleteAsync(
                $"/api/admin/categories/{Guid.NewGuid()}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WhenCategoryHasReferencedTransactions_Returns409Conflict()
        {
            // Arrange
            const string ownerId = "user-1";
            var categoryId = Guid.NewGuid();
            using var client = await _fixture.CreateAdminClientAsync("admin-1");

            await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.Add(new AppUser { Id = ownerId });
                db.Categories.Add(new Category { Id = categoryId, Name = "Books", AppUserId = ownerId });
                db.Transactions.Add(new FinancialTransaction
                {
                    CategoryId = categoryId,
                    Type = FinancialTransactionType.Expense,
                    Amount = 100m,
                    AppUserId = ownerId
                });
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.DeleteAsync(
                $"/api/admin/categories/{categoryId}",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}

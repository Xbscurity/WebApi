using api.Data;
using api.Models;
using api.Tests.Integration.Auth;
using api.Tests.Integration.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using System.Text.Json;
using Testcontainers.PostgreSql;
using ZiggyCreatures.Caching.Fusion;

namespace api.Tests.Integration.Collections.Fixtures
{
    public class IntegrationTestFixture : IAsyncLifetime
    {
        public DateTimeOffset CurrentTime
        {
            get => Factory.CurrentTime;
            set => Factory.CurrentTime = value;
        }

        private readonly PostgreSqlContainer _postgresContainer =
            new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        private Respawner _respawner = default!;
        private NpgsqlConnection _respawnConnection = default!;

        public CustomWebApplicationFactory Factory { get; private set; } = default!;
        public string ConnectionString { get; private set; } = default!;
        public JsonSerializerOptions JsonOptions => Factory.JsonOptions;

        public HttpClient CreateAnonymousClient() => Factory.CreateClient();

        public Task<HttpClient> CreateUserClientAsync(string? userId, bool isBanned = false)
        => CreateAndSeedClientAsync(userId, "User", isBanned);

        public Task<HttpClient> CreateAdminClientAsync(string userId, bool isBanned = false)
            => CreateAndSeedClientAsync(userId, "Admin", isBanned);

        public async Task<HttpClient> CreateAndSeedClientAsync(string? userId, string role, bool isBanned = false)
        {
            var finalUserId = userId ?? Guid.NewGuid().ToString()[..8];
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var user = new AppUser
                {
                    Id = finalUserId,
                    UserName = $"{role.ToLower()}_{finalUserId}@test.com",
                    Email = $"{role.ToLower()}_{finalUserId}@test.com",
                    IsBanned = isBanned
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, finalUserId);

            return client;
        }

        public async ValueTask InitializeAsync()
        {
            await _postgresContainer.StartAsync();

            ConnectionString = _postgresContainer.GetConnectionString();

            Factory = new CustomWebApplicationFactory
            {
                ConnectionString = ConnectionString
            };

            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
            }

            _respawnConnection = new NpgsqlConnection(ConnectionString);
            await _respawnConnection.OpenAsync();

            _respawner = await Respawner.CreateAsync(_respawnConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore =
                [
                    new Table("__EFMigrationsHistory"),
                    new Table("AspNetRoles")
                    ],
                WithReseed = true,
            });
        }

        public async Task ResetCheckpointAsync()
        {
            await _respawner.ResetAsync(_respawnConnection);
            Factory.ResetTime();
            using var scope = Factory.Services.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
            await cache.ClearAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_respawnConnection is not null)
                await _respawnConnection.DisposeAsync();

            if (Factory is not null)
                await Factory.DisposeAsync();

            await _postgresContainer.DisposeAsync();

            GC.SuppressFinalize(this);
        }
    }
}
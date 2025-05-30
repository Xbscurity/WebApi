using api.Data;
using api.Tests.Integration.Factories;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class IntegrationTestFixture : IAsyncLifetime
{
    private IContainer _postgresContainer = default!;
    public CustomWebApplicationFactory Factory { get; private set; } = default!;
    public HttpClient Client => Factory.CreateClient();
    public string ConnectionString = default!;

    public async Task InitializeAsync()
    {
        _postgresContainer = new ContainerBuilder()
            .WithImage("postgres:15")
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithEnvironment("POSTGRES_DB", "testdb")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        await _postgresContainer.StartAsync();

        var port = _postgresContainer.GetMappedPublicPort(5432);
        var host = _postgresContainer.Hostname;

        ConnectionString = $"Host={host};Port={port};Database=testdb;Username=postgres;Password=postgres";

        Factory = new CustomWebApplicationFactory
        {
            ConnectionString = ConnectionString
        };

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.StopAsync();
    }
}

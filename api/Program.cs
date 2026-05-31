using api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    Log.Information("Starting up...");

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MinRequestBodyDataRate =
            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(5));

        options.Limits.MinResponseDataRate =
            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(5));

        options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
    });

    builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWeb();

    var app = builder.Build();

    await app.InitializeDatabaseAsync();

    app.UseWebPipeline();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Partial <see cref="Program"/> class used as an entry point
/// for integration testing with <c>WebApplicationFactory&lt;TEntryPoint&gt;</c>.
/// </summary>
public partial class Program
{
}

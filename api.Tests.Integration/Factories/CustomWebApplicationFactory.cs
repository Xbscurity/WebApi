using api.Data;
using api.Providers.Time;
using api.Tests.Integration.Auth;
using api.Tests.Integration.TestControllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;

namespace api.Tests.Integration.Factories
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string ConnectionString { get; set; } = default!;
        public Mock<ITimeProvider> TimeProviderMock { get; } = new();
        public DateTimeOffset CurrentTime { get; set; } = DateTimeOffset.UtcNow;

        private JsonSerializerOptions? _jsonOptions;
        public JsonSerializerOptions JsonOptions =>
            _jsonOptions ??= Services
                .GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>()
                .Value.JsonSerializerOptions;
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(ConnectionString));

                TimeProviderMock.Setup(x => x.UtcNow).Returns(() => CurrentTime);
                services.RemoveAll<ITimeProvider>();
                services.AddSingleton(TimeProviderMock.Object);

                services.RemoveAll<IHostedService>();

                services.AddControllers()
                    .AddApplicationPart(typeof(TestExceptionController).Assembly)
                    .AddControllersAsServices();

                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });

                services.Configure<RateLimiterOptions>(options =>
                {
                    options.GlobalLimiter = null;
                });
            });
        }

        public void ResetTime()
        {
            CurrentTime = DateTimeOffset.UtcNow;
        }
    }
}
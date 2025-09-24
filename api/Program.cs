using api.Authorization;
using api.Constants;
using api.Data;
using api.Filters;
using api.Middlewares;
using api.Models;
using api.Providers;
using api.Providers.Interfaces;
using api.Repositories.Categories;
using api.Repositories.Interfaces;
using api.Services.Background;
using api.Services.Categories;
using api.Services.Token;
using api.Services.Transaction;
using api.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;
try
{
    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Seq("http://seq")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (IP: {ClientIp}, UA: {UserAgent}){NewLine}{Exception}")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

    Log.Information("Starting up...");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


    builder.Host.UseSerilog();
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
        options.Filters.Add<StatusCodeFilter>();
    });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
    builder.Services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
    builder.Services.AddScoped<IGroupingReportStrategy, GroupByCategoryStrategy>();
    builder.Services.AddScoped<IGroupingReportStrategy, GroupByDateStrategy>();
    builder.Services.AddScoped<IGroupingReportStrategy, GroupByDateAndCategoryStrategy>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSingleton<FinancialTransactionSortValidator>();
    builder.Services.AddSingleton<CategorySortValidator>();
    builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
    builder.Services.AddSingleton<ITimeProvider, UtcTimeProvider>();

    builder.Services.AddHostedService<RefreshTokenCleanupService>();

    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = Path.Combine(AppContext.BaseDirectory, "ApiComments.xml");
        options.IncludeXmlComments(xmlFile);
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter JWT token in the format: Bearer {your token}",
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
        });

        // options.OperationFilter<CustomTimeZoneParameterFilter>();
    });
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
    builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<SignInManager<AppUser>>();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = JwtRegisteredClaimNames.Name,

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])),
            RequireSignedTokens = true,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha512 },
        };
    });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(Policies.Admin, policy =>
        {
            policy.RequireRole(Roles.Admin);
        })
        .AddPolicy(Policies.UserNotBanned, policy =>
        {
            policy.RequireRole(Roles.User);
            policy.Requirements.Add(new NotBannedRequirement());
        })
        .AddPolicy(Policies.CategoryAccessGlobal, policy =>
        policy.Requirements.Add(new CategoryAccessRequirement(allowGlobal: true)))

        .AddPolicy(Policies.CategoryAccessNoGlobal, policy =>
    policy.Requirements.Add(new CategoryAccessRequirement(allowGlobal: false)))

        .AddPolicy(Policies.TransactionAccess, policy =>
    policy.Requirements.Add(new TransactionAccessRequirement()));

    builder.Services.AddScoped<IAuthorizationHandler, NotBannedHandler>();
    builder.Services.AddScoped<IAuthorizationHandler, CategoryAccessHandler>();
    builder.Services.AddScoped<IAuthorizationHandler, TransactionAccessHandler>();

    builder.Services.AddHealthChecks();

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.Identity?.Name
              ?? httpContext.Connection.RemoteIpAddress?.ToString()
              ?? "unknown",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }));
    });

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 1 * 1024 * 1024; // 1 MB
        options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10);
        options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);

        options.Limits.MinRequestBodyDataRate =
    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(5));
        options.Limits.MinResponseDataRate =
            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(5));
    });

    // TypeDescriptor.AddAttributes(typeof(TimeZoneInfo), new TypeConverterAttribute(typeof(TimeZoneInfoConverter)));
    var app = builder.Build();

    app.UseMiddleware<ErrorHandlingMiddleware>();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    app.MapHealthChecks("/health");

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DataSeeder.SeedRolesAndAdminAsync(roleManager, userManager);
        await DataSeeder.SeedAppDataAsync(context);
    }

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<LogEnrichmentMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            diagnosticContext.Set("UserAgent", userAgent);
        };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapControllers();
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

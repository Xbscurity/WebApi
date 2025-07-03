using api.Authorization;
using api.Constants;
using api.Data;
using api.Filters;
using api.Middlewares;
using api.Models;
using api.Providers;
using api.Providers.Interfaces;
using api.Repositories;
using api.Repositories.Interfaces;
using api.Services;
using api.Services.Interfaces;
using api.Services.Transaction;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionFilter>();
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<StatusCodeFilter>();

    // options.ModelBinderProviders.Insert(0, new TimeZoneInfoModeBinderProvider());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddScoped<ExecutionTimeFilter>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IGroupingReportStrategy, GroupByCategoryStrategy>();
builder.Services.AddScoped<IGroupingReportStrategy, GroupByDateStrategy>();
builder.Services.AddScoped<IGroupingReportStrategy, GroupByDateAndCategoryStrategy>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
builder.Services.AddSingleton<ITimeProvider, UtcTimeProvider>();
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
        NameClaimType = JwtRegisteredClaimNames.Sub,
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
    });

builder.Services.AddScoped<IAuthorizationHandler, NotBannedHandler>();

// TypeDescriptor.AddAttributes(typeof(TimeZoneInfo), new TypeConverterAttribute(typeof(TimeZoneInfoConverter)));
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var context = services.GetRequiredService<ApplicationDbContext>();
    await DataSeeder.SeedRolesAndAdminAsync(roleManager, userManager);
    await DataSeeder.SeedAppDataAsync(context);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

public partial class Program { }

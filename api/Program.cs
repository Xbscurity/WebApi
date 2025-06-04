using api.Data;
using api.Filters;
using api.Providers;
using api.Providers.Interfaces;
using api.Repositories;
using api.Repositories.Interfaces;
using api.Services;
using api.Services.Interfaces;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionFilter>();
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<StatusCodeFilter>();

    // options.ModelBinderProviders.Insert(0, new TimeZoneInfoModeBinderProvider());
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
builder.Services.AddSingleton<ITimeProvider, UtcTimeProvider>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "ApiComments.xml");
    options.IncludeXmlComments(xmlFile);

    // options.OperationFilter<CustomTimeZoneParameterFilter>();
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// TypeDescriptor.AddAttributes(typeof(TimeZoneInfo), new TypeConverterAttribute(typeof(TimeZoneInfoConverter)));
var app = builder.Build();
app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

public partial class Program { }

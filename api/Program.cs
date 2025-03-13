using System.ComponentModel;
using api.Controllers;
using api.Converters;
using api.Data;
using api.Filters;
using api.ModelBinders;
using api.Repositories;
using api.Repositories.Interfaces;
using api.Swagger;
using Azure.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddControllers(options =>
{
    //options.Filters.Add<ExceptionFilter>();
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<StatusCodeFilter>();
    //options.ModelBinderProviders.Insert(0, new TimeZoneInfoModeBinderProvider());
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddScoped<ExecutionTimeFilter>();
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "ApiComments.xml");
    options.IncludeXmlComments(xmlFile);
    //options.OperationFilter<CustomTimeZoneParameterFilter>();
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//TypeDescriptor.AddAttributes(typeof(TimeZoneInfo), new TypeConverterAttribute(typeof(TimeZoneInfoConverter)));
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

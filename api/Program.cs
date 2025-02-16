using api.Data;
using api.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
     var xmlFile = Path.Combine(AppContext.BaseDirectory, "ApiComments.xml"); 
    options.IncludeXmlComments(xmlFile);  
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
var app = builder.Build();
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>(); 
app.UseMiddleware<StatusCodeMiddleware>(); 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();
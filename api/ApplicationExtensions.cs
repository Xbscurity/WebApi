using api.Data;
using api.Models;
using api.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace api
{
    /// <summary>
    /// Provides extension methods for application initialization tasks.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Applies database migrations and seeds initial data.
        /// </summary>
        /// <param name="app">The web application instance.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var seedOptions = services.GetRequiredService<IOptions<SeedOptions>>().Value;

            await DataSeeder.SeedRolesAndAdminAsync(roleManager, userManager, seedOptions);
        }
    }
}

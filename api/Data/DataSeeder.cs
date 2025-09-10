using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Data
{
    /// <summary>
    /// Provides methods to seed initial roles, admin user, and application data into the database.
    /// </summary>
    public static class DataSeeder
    {
        /// <summary>
        /// Seeds default roles ("Admin" and "User") and an admin user if they do not exist.
        /// </summary>
        /// <param name="roleManager">The <see cref="RoleManager{IdentityRole}"/> used to manage roles.</param>
        /// <param name="userManager">The <see cref="UserManager{AppUser}"/> used to manage users.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown if role or admin user creation fails.</exception>
        public static async Task SeedRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var identityRole = new IdentityRole(role)
                    {
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    var roleResult = await roleManager.CreateAsync(identityRole);

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Failed to create role '{role}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                };

                var createResult = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (!createResult.Succeeded)
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                var addRoleResult = await userManager.AddToRoleAsync(newAdmin, "Admin");

                if (!addRoleResult.Succeeded)
                {
                    throw new Exception($"Failed to add admin role to user: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                }
            }
        }

        /// <summary>
        /// Seeds initial application data, such as default categories, if they do not exist.
        /// </summary>
        /// <param name="context">The <see cref="ApplicationDbContext"/> used to access the database.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SeedAppDataAsync(ApplicationDbContext context)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Food" },
                    new Category { Name = "Transport" },
                    new Category { Name = "Entertainment" },
                    new Category { Name = "Bills" },
                    new Category { Name = "Health" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}

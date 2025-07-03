using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Data
{
    public static class DataSeeder
    {
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
                    EmailConfirmed = true
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

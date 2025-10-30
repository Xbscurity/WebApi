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
        /// Defines the structure for a category template used during user creation.
        /// This record holds the base properties to be copied to a new user's category entity.
        /// </summary>
        public record SeedCategoryTemplate
        {
            /// <summary>
            /// Gets the default name for the category.
            /// </summary>
            public string Name { get; init; } = default!;
        }

        /// <summary>
        /// A static, read-only list of predefined categories that will be created
        /// for every new user upon registration.
        /// </summary>
        public static readonly List<SeedCategoryTemplate> DefaultCategoryTemplates = new()
    {
        new() { Name = "Food" },
        new() { Name = "Transport" },
        new() { Name = "Entertainment" },
        new() { Name = "Bills" },
        new() { Name = "Health" },
    };

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
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
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
    }
}

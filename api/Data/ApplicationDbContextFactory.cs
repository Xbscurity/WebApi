using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace api.Data
{
    /// <summary>
    /// Provides a factory for creating <see cref="ApplicationDbContext"/> instances at design time.
    /// This is used by tools such as Entity Framework Core CLI for migrations.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ApplicationDbContext"/> with the configured options.
        /// </summary>
        /// <param name="args">Command-line arguments (not used).</param>
        /// <returns>A new <see cref="ApplicationDbContext"/> instance.</returns>
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings files and environment variables
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

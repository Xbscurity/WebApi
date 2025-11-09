using api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the application,
    /// including identity users and application-specific entities.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class
        /// using the specified options.
        /// </summary>
        /// <param name="options">The options to configure the database context.</param>
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the categories in the database.
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Gets or sets the financial transactions in the database.
        /// </summary>
        public DbSet<FinancialTransaction> Transactions { get; set; }

        /// <summary>
        /// Gets or sets the refresh tokens in the database.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Configures the entity mappings, relationships, and database-specific behaviors.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure FinancialTransaction -> Category relationship with nullable foreign key
            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(transaction => transaction.Category)
                .WithMany()
                .HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FinancialTransaction>()
                .Property(t => t.CreatedAt)
                .HasColumnType("timestamptz");

            modelBuilder.Entity<FinancialTransaction>()
                .Property(t => t.CreatedAt)
                .HasConversion(
                    source => source.ToUniversalTime(),
                    stored => stored.ToLocalTime());

            // Ensure RefreshToken.TokenHash is unique
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash)
                .IsUnique();

            modelBuilder.Entity<Category>()
            .HasQueryFilter(c => c.IsActive);
        }
    }
}

using api.Models;
using api.Providers.Time;
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
        private readonly ITimeProvider _timeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class
        /// using the specified options.
        /// </summary>
        /// <param name="options">The options to configure the database context.</param>
        /// <param name="timeProvider">The provider used to obtain the current UTC time.</param>
        public ApplicationDbContext(
            DbContextOptions options,
            ITimeProvider timeProvider)
            : base(options)
        {
            _timeProvider = timeProvider;
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

        /// <inheritdoc/>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = _timeProvider.UtcNow;

            var trackedEntries = ChangeTracker.Entries<ITrackedEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in trackedEntries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                }

                entry.Entity.UpdatedAt = now;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Configures the entity mappings, relationships, and database-specific behaviors.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .Ignore(rt => rt.IsRevoked);

            modelBuilder.Entity<RefreshToken>()
                .HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(rt => rt.UserId);

            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.AppUserId, c.Name })
                .IsUnique();

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .HasMaxLength(20);

            modelBuilder.Entity<FinancialTransaction>()
                .Property(ft => ft.Comment)
                .HasMaxLength(255);

            modelBuilder.Entity<FinancialTransaction>()
                .Property(ft => ft.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<FinancialTransaction>()
                .Property(t => t.Amount)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(transaction => transaction.Category)
                .WithMany()
                .HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

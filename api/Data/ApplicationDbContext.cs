using api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {

        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<FinancialTransaction> Transactions { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<RefreshToken>()
     .HasIndex(rt => rt.TokenHash)
     .IsUnique();
        }
    }
}
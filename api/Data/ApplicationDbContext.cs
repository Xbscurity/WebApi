using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        private readonly ICurrentUserService _currentUser;

        public ApplicationDbContext(DbContextOptions options, ICurrentUserService currentUser)
            : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<FinancialTransaction> Transactions { get; set; }

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

            //modelBuilder.Entity<Category>().HasQueryFilter(c => c.IsActive);
        }
    }
}
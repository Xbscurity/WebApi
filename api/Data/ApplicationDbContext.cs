using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FinancialTransaction> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();
            });

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
               stored => stored.ToLocalTime()
           );
        }
    }
}
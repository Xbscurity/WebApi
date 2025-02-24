using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }
        public DbSet<Category> Categories {get;set;}
        public DbSet<FinancialTransaction> Transactions{get;set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<FinancialTransaction>()
        .HasOne(t => t.Category)
        .WithMany() 
        .HasForeignKey(t => t.CategoryId)
        .OnDelete(DeleteBehavior.SetNull); 
}
    }
}
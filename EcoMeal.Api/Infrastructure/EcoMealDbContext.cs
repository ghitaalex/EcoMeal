using EcoMeal.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.Api.Infrastructure
{
    public class EcoMealDbContext : DbContext
    {
        public EcoMealDbContext(DbContextOptions<EcoMealDbContext> options) : base(options)
        {
        }
        public DbSet<User> User { get; set; }
        public DbSet<BusinessType> BusinessType { get; set; }
        public DbSet<PackageType> PackageType { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Business>().HasKey(e => e.Id);

            modelBuilder.Entity<Business>()
                .HasOne(p => p.BusinessType)
                .WithMany(p => p.Businesses)
                .HasForeignKey(p => p.BusinessTypeId);
        }
    }
}

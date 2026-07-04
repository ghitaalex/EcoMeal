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
        public DbSet<Business> Business { get; set; }
        public DbSet<Package> Package { get; set; }
        public DbSet<Order> Order { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Business>().HasKey(e => e.Id);

            modelBuilder.Entity<Business>()
                .HasOne(p => p.BusinessType)
                .WithMany(p => p.Businesses)
                .HasForeignKey(p => p.BusinessTypeId);

            modelBuilder.Entity<Order>().HasKey(e => e.Id);

            modelBuilder.Entity<Order>()
                .HasOne(p => p.User)
                .WithMany(p => p.Orders)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Order>()
                .HasOne(p => p.Package)
                .WithMany(p => p.Orders)
                .HasForeignKey(p => p.PackageId);

            modelBuilder.Entity<Package>().HasKey(e => e.Id);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Business)
                .WithMany(p => p.Packages)
                .HasForeignKey(p => p.BusinessId);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.PackageType)
                .WithMany(p => p.Packages)
                .HasForeignKey(p => p.PackageTypeId);

            modelBuilder.Entity<Package>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
        }
    }
}

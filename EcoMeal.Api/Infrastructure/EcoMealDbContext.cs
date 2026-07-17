using EcoMeal.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.Api.Infrastructure
{
    public class EcoMealDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public EcoMealDbContext(DbContextOptions<EcoMealDbContext> options) : base(options)
        {
        }
        public DbSet<BusinessType> BusinessType { get; set; }
        public DbSet<PackageType> PackageType { get; set; }
        public DbSet<Business> Business { get; set; }
        public DbSet<Package> Package { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Review> Review { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Business>().HasKey(e => e.Id);

            modelBuilder.Entity<Business>()
                .HasOne(p => p.BusinessType)
                .WithMany(p => p.Businesses)
                .HasForeignKey(p => p.BusinessTypeId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.FavoriteBusinesses)
                .WithMany(b => b.FavoritedByUsers)
                .UsingEntity(j => j.ToTable("FavoriteBusiness"));

            modelBuilder.Entity<Order>().HasKey(e => e.Id);

            modelBuilder.Entity<Order>()
                .HasOne(p => p.User)
                .WithMany(p => p.Orders)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Order>()
                .HasOne(p => p.Package)
                .WithMany(p => p.Orders)
                .HasForeignKey(p => p.PackageId);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

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

            modelBuilder.Entity<Review>().HasKey(e => e.Id);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.OrderId)
                .IsUnique();
        }
    }
}

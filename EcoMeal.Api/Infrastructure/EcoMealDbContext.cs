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
    }
}

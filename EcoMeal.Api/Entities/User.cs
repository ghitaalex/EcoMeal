using Microsoft.AspNetCore.Identity;

namespace EcoMeal.Api.Entities
{
    public class User : IdentityUser<int>
    {
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

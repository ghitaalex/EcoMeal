using System.ComponentModel.DataAnnotations;

namespace EcoMeal.Api.Entities
{
    public class BusinessType
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public required string Name { get; set; }
        public ICollection<Business> Businesses { get; set; } = new List<Business>();
    }
}

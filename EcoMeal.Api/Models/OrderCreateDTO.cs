using System.ComponentModel.DataAnnotations;

namespace EcoMeal.Api.Models
{
    public class OrderCreateDTO
    {
        [Required]
        public int PackageId { get; set; }
    }
}

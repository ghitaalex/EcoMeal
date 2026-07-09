using System.ComponentModel.DataAnnotations;

namespace EcoMeal.Client.Models
{
    public class BusinessAddModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        public required string Address { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Contact is required")]
        [StringLength(100)]
        public required string Contact { get; set; }

        [Required]
        public int BusinessTypeId { get; set; }
    }
}

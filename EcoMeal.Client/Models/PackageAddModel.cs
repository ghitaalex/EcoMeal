using System.ComponentModel.DataAnnotations;

namespace EcoMeal.Client.Models
{
    public class PackageAddModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50)]
        public required string Name { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [StringLength(250)]
        public required string Description { get; set; }
        [Required]
        [Range(0.01, 1000, ErrorMessage = "Price must be between 0.01 and 1000")]
        public double Price { get; set; }
        [Required]
        public DateTime StartPickup { get; set; }
        [Required]
        public DateTime EndPickup { get; set; }
        [Required]
        public int PackageTypeId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace EcoMeal.Client.Models
{
    public class PackageAddModel
    {
        [Required(ErrorMessage = "Nume este obligatoriu")]
        [StringLength(50)]
        public required string Name { get; set; }
        [Required(ErrorMessage = "Descrierea este obligatorie")]
        [StringLength(250)]
        public required string Description { get; set; }
        [Required]
        [Range(0, 1000)]
        public double Price { get; set; }
        [Required]
        public DateTime StartPickup { get; set; }
        [Required]
        public DateTime EndPickup { get; set; }
        [Required]
        public int PackageTypeId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace EcoMeal.Api.Models
{
    public class PackageAddDTO
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Number of packages must be at least 1")]
        public int NoPackages { get; set; }
        public DateTime StartPickup { get; set; }
        public DateTime EndPickup { get; set; }
        public int PackageTypeId { get; set; }
        public IFormFile? PackageImage { get; set; }
    }
}

namespace EcoMeal.Api.Models
{
    public class PackageDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime PickUpStart { get; set; }
        public DateTime PickUpEnd { get; set; }
        public required string PackageTypeName { get; set; }
    }
}

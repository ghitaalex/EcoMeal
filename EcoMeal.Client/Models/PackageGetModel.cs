namespace EcoMeal.Client.Models
{
    public class PackageGetModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int NoPackages { get; set; }
        public int AvailablePackages { get; set; }
        public DateTime PickUpStart { get; set; }
        public DateTime PickUpEnd { get; set; }
        public string PackageTypeName { get; set; } = "";
        public string? PackageImageUrl { get; set; }
    }
}

namespace EcoMeal.Api.Entities
{
    public class Package
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int NoPackages { get; set; }
        public int BusinessId { get; set; }
        public Business Business { get; set; }
        public int PackageTypeId { get; set; }
        public PackageType PackageType { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime PickUpStart { get; set; }
        public DateTime PickUpEnd { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

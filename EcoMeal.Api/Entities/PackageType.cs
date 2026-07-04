namespace EcoMeal.Api.Entities
{
    public class PackageType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Package> Packages { get; set; } = new List<Package>();
    }
}

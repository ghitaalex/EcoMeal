namespace EcoMeal.Client.Models
{
    public class BusinessDetailsModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Adress { get; set; } = "";
        public string? Description { get; set; }
        public string Contact { get; set; } = "";
        public string BusinessTypeName { get; set; } = "";
        // public IEnumerable<PackageGetModel> Packages { get; set; } = new List<PackageGetModel>();

    }
}
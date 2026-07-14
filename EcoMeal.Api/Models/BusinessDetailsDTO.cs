namespace EcoMeal.Api.Models
{
    public class BusinessDetailsDTO : BusinessDTO
    {
        public IEnumerable<PackageDTO> Packages { get; set; }
    }
}

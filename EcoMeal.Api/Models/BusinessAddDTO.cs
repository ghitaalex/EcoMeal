namespace EcoMeal.Api.Models
{
    public class BusinessAddDTO
    {
        public required string Name { get; set; }
        public required string Address { get; set; }
        public string? Description { get; set; }
        public required string Contact { get; set; }
        public int BusinessTypeId { get; set; }
    }
}

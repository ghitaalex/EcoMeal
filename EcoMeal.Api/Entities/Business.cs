using System.ComponentModel.DataAnnotations.Schema;

namespace EcoMeal.Api.Entities
{
    public class Business
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public string? Description { get; set; }
        public required string Contact { get; set; }
        [ForeignKey(nameof(BusinessType))]
        public int BusinessTypeId { get; set; }
        public required BusinessType BusinessType { get; set; }
    }
}

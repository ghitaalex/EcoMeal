namespace EcoMeal.Client.Models
{
    public class BusinessModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public string? Description { get; set; }
        public required string Contact { get; set; }
        public required string BusinessTypeName { get; set; }
        public string? BusinessImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}

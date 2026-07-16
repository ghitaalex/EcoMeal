namespace EcoMeal.Client.Models
{
    public class BusinessModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Description { get; set; }
        public required string Contact { get; set; }
        public required string BusinessTypeName { get; set; }
        public string? BusinessImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsFavorite { get; set; }
        public double? DistanceKm { get; set; }
        public double? DurationMinutes { get; set; }
    }
}

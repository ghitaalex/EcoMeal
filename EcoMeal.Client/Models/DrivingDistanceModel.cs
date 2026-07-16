namespace EcoMeal.Client.Models
{
    public class UserLocationModel
    {
        public bool Success { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? AccuracyMeters { get; set; }
        public string? Error { get; set; }
    }

    public class DrivingDistanceModel
    {
        public int BusinessId { get; set; }
        public double DistanceKm { get; set; }
        public double DurationMinutes { get; set; }
    }

    public class MapBusinessMarkerModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string BusinessTypeName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? DistanceLabel { get; set; }
    }
}

namespace EcoMeal.Api.Models
{
    public class UserLocationDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class DrivingDistanceDTO
    {
        public int BusinessId { get; set; }
        public double DistanceKm { get; set; }
        public double DurationMinutes { get; set; }
    }
}

using System.Net.Http.Json;
using EcoMeal.Api.Models;

namespace EcoMeal.Api.Services
{
    public class DrivingDistanceService
    {
        private readonly HttpClient _httpClient;

        public DrivingDistanceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DrivingDistanceDTO>> GetDrivingDistancesAsync(
            double userLatitude,
            double userLongitude,
            List<BusinessLocation> businesses)
        {
            if (businesses.Count == 0)
                return new List<DrivingDistanceDTO>();

            var locations = new List<double[]>
            {
                new[] { userLongitude, userLatitude }
            };

            locations.AddRange(businesses.Select(b => new[] { b.Longitude, b.Latitude }));

            var requestBody = new MatrixRequest
            {
                Locations = locations,
                Sources = new[] { 0 },
                Destinations = Enumerable.Range(1, businesses.Count).ToArray(),
                Metrics = new[] { "distance", "duration" }
            };

            var response = await _httpClient.PostAsJsonAsync("v2/matrix/driving-car", requestBody);
            if (!response.IsSuccessStatusCode)
                return new List<DrivingDistanceDTO>();

            var matrix = await response.Content.ReadFromJsonAsync<MatrixResponse>();
            var distances = matrix?.Distances.FirstOrDefault();
            var durations = matrix?.Durations.FirstOrDefault();

            if (distances == null || durations == null)
                return new List<DrivingDistanceDTO>();

            var results = new List<DrivingDistanceDTO>();
            for (var i = 0; i < businesses.Count; i++)
            {
                if (i >= distances.Length || i >= durations.Length ||
                    !distances[i].HasValue || !durations[i].HasValue)
                {
                    continue;
                }

                results.Add(new DrivingDistanceDTO
                {
                    BusinessId = businesses[i].Id,
                    DistanceKm = distances[i]!.Value / 1000,
                    DurationMinutes = durations[i]!.Value / 60
                });
            }

            return results;
        }

        private class MatrixRequest
        {
            public required List<double[]> Locations { get; set; }
            public required int[] Sources { get; set; }
            public required int[] Destinations { get; set; }
            public required string[] Metrics { get; set; }
        }

        private class MatrixResponse
        {
            public double?[][] Distances { get; set; } = [];
            public double?[][] Durations { get; set; } = [];
        }
    }

    public class BusinessLocation
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

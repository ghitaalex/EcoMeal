using System.Globalization;
using System.Text.Json.Serialization;

namespace EcoMeal.Api.Services
{
    public class GeocodingService
    {
        private readonly HttpClient _httpClient;

        public GeocodingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Coordinates?> GeocodeAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            try
            {
                var requestUrl = $"search?format=jsonv2&limit=1&countrycodes=ro&q={Uri.EscapeDataString(address)}";

                var results = await _httpClient.GetFromJsonAsync<List<NominatimResult>>(requestUrl);
                var result = results?.FirstOrDefault();

                if (result == null)
                    return null;

                return new Coordinates
                {
                    Latitude = double.Parse(result.Latitude),
                    Longitude = double.Parse(result.Longitude)
                };
            }
            catch
            {
                return null;
            }
        }

        private class NominatimResult
        {
            [JsonPropertyName("lat")]
            public string Latitude { get; set; } = string.Empty;

            [JsonPropertyName("lon")]
            public string Longitude { get; set; } = string.Empty;
        }
    }

    public class Coordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

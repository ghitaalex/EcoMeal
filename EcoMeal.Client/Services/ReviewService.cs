using EcoMeal.Client.Models;

namespace EcoMeal.Client.Services;

public class ReviewService
{
    private readonly HttpClient _httpClient;

    public ReviewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ReviewGetModel>> GetByBusinessAsync(int businessId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<ReviewGetModel>>($"api/review/business/{businessId}");
        return response ?? new List<ReviewGetModel>();
    }

    public async Task<bool> CreateReviewAsync(ReviewRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/review", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteReviewAsync(int reviewId)
    {
        var response = await _httpClient.DeleteAsync($"api/review/{reviewId}");
        return response.IsSuccessStatusCode;
    }
}

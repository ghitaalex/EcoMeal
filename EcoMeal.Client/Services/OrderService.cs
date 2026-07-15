using System.Net.Http.Json;
using EcoMeal.Client.Models;

namespace EcoMeal.Client.Services;

public class OrderService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public OrderService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<(bool Success, string? Error)> PlaceOrderAsync(int packageId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/order", new { PackageId = packageId });
            if (response.IsSuccessStatusCode)
                return (true, null);

            var body = await response.Content.ReadAsStringAsync();
            return (false, $"Status {(int)response.StatusCode}: {body}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<OrderGetModel>> GetMyOrderAsync()
    {
        var response = await _httpClient.GetAsync("api/order");
        if (!response.IsSuccessStatusCode)
            return new List<OrderGetModel>();
        return await response.Content.ReadFromJsonAsync<List<OrderGetModel>>() ?? new List<OrderGetModel>();
    }

    public async Task<List<OrderGetModel>> GetOrdersByBusinessAsync(int businessId)
    {
        var response = await _httpClient.GetAsync($"api/order/business/{businessId}");
        if (!response.IsSuccessStatusCode)
            return new List<OrderGetModel>();
        return await response.Content.ReadFromJsonAsync<List<OrderGetModel>>() ?? new List<OrderGetModel>();
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/order/{orderId}/status", status);
        return response.IsSuccessStatusCode;
    }
}
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
            var response = await _httpClient.PostAsJsonAsync("api/order", new
            {
                PackageId = packageId,
                PayWithCard = false
            });

            return response.IsSuccessStatusCode
                ? (true, null)
                : (false, "Could not place the order.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(OrderCheckoutModel? Checkout, string? Error)> StartCardCheckoutAsync(int packageId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/order", new
            {
                PackageId = packageId,
                PayWithCard = true
            });

            if (!response.IsSuccessStatusCode)
                return (null, "Could not start Stripe Checkout.");

            var checkout = await response.Content.ReadFromJsonAsync<OrderCheckoutModel>();
            return string.IsNullOrWhiteSpace(checkout?.CheckoutUrl)
                ? (null, "Stripe did not return a checkout link.")
                : (checkout, null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
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

namespace EcoMeal.Client.Models;

public sealed class OrderCheckoutModel
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CheckoutUrl { get; set; }
}

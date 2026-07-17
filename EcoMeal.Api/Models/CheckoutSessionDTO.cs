namespace EcoMeal.Api.Models
{
    public class CheckoutSessionDTO
    {
        public int OrderId { get; set; }
        public required string Status { get; set; }
        public string? CheckoutUrl { get; set; }
    }
}

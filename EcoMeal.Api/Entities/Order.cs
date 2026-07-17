namespace EcoMeal.Api.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
        public required string Status { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public bool StockReserved { get; set; }
    }
}

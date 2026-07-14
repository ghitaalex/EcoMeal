namespace EcoMeal.Api.Models
{
    public class OrderGetDTO
    {
        public int Id { get; set; }
        public string PackageName { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public DateTime Date { get; set; }
        public string? UserName { get; set; }
        public string? UserContact { get; set; }
    }
}

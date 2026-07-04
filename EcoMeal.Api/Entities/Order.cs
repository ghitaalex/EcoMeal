namespace EcoMeal.Api.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
        public int PackageId { get; set; }
        public required Package Package { get; set; }
        public required string Status { get; set; }
        public DateTime Date { get; set; }
    }
}

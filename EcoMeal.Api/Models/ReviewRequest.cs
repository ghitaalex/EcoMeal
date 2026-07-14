namespace EcoMeal.Api.Models
{
    public class ReviewRequest
    {
        public int BusinessId { get; set; }
        public int Rating { get; set; }
        public string? Review { get; set; }
    }
}

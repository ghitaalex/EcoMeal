namespace EcoMeal.Client.Models
{
    public class SearchResult
    {
        public string ResultType { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = "";
        public string NavigateUrl { get; set; } = "";
        public string? ExtraInfo { get; set; }
    }
}

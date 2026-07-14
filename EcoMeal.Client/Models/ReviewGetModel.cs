namespace EcoMeal.Client.Models;

public class ReviewGetModel
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = "";
}

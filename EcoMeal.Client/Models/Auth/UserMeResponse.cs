namespace EcoMeal.Client.Models.Auth;

public class UserMeResponse
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Contact { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}

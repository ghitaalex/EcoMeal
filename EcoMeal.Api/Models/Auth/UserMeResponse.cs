namespace EcoMeal.Api.Models.Auth;

public class UserMeResponse
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Contact { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

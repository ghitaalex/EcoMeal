namespace EcoMeal.Client.Models.Auth;

public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static AuthResult Ok() => new() { Success = true };
    public static AuthResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}

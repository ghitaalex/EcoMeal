using System.Net.Http.Json;
using EcoMeal.Client.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace EcoMeal.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public string? Token { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public AuthService(HttpClient http, ProtectedLocalStorage localStorage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string name, string contact)
    {
        var request = new RegisterRequest { Email = email, Password = password, Name = name, Contact = contact };
        var response = await _http.PostAsJsonAsync("api/auth/register", request);

        if (response.IsSuccessStatusCode)
            return AuthResult.Ok();

        var error = await response.Content.ReadFromJsonAsync<RegisterErrorResponse>();
        var errorMessage = error?.Errors != null
            ? string.Join("; ", error.Errors)
            : "Registration failed.";

        return AuthResult.Fail(errorMessage);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var request = new AuthRequest { Email = email, Password = password };
        var response = await _http.PostAsJsonAsync("login", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Token = result?.AccessToken;

            if (Token != null)
            {
                await _localStorage.SetAsync("authToken", Token);

                var roles = await FetchRolesAsync(Token);
                await _localStorage.SetAsync("userRoles", roles);

                if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                {
                    customProvider.NotifyUserAuthentication(Token, roles);
                }
            }

            return AuthResult.Ok();
        }

        return AuthResult.Fail("Invalid email or password.");
    }

    public async Task LoadTokenAsync()
    {
        var tokenResult = await _localStorage.GetAsync<string>("authToken");
        Token = tokenResult.Success ? tokenResult.Value : null;

        if (Token != null)
        {
            var rolesResult = await _localStorage.GetAsync<List<string>>("userRoles");
            var roles = rolesResult.Success && rolesResult.Value != null ? rolesResult.Value : new List<string>();

            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyUserAuthentication(Token, roles);
            }
        }
    }

    public async Task LogoutAsync()
    {
        Token = null;
        await _localStorage.DeleteAsync("authToken");
        await _localStorage.DeleteAsync("userRoles");

        if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
        {
            customProvider.NotifyUserLogout();
        }
    }

    private async Task<List<string>> FetchRolesAsync(string token)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var userMe = await response.Content.ReadFromJsonAsync<UserMeResponse>();
                return userMe?.Roles ?? new List<string>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching roles: {ex.Message}");
        }

        return new List<string>();
    }
}

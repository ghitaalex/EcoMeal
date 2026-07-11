using System.Net.Http.Json;
using System.Net.Http.Headers;
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
    public string? UserName { get; private set; }
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
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
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
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var rolesResult = await _localStorage.GetAsync<List<string>>("userRoles");
            var roles = rolesResult.Success && rolesResult.Value != null ? rolesResult.Value : new List<string>();

            var nameResult = await _localStorage.GetAsync<string>("userName");
            UserName = nameResult.Success ? nameResult.Value : null;

            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyUserAuthentication(Token, roles);
            }
        }
    }

    public async Task LogoutAsync()
    {
        Token = null;
        UserName = null;
        _http.DefaultRequestHeaders.Authorization = null;
        await _localStorage.DeleteAsync("authToken");
        await _localStorage.DeleteAsync("userRoles");
        await _localStorage.DeleteAsync("userName");

        if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
        {
            customProvider.NotifyUserLogout();
        }
    }

    public async Task<UserMeResponse?> GetProfileAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/auth/me");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserMeResponse>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching profile: {ex.Message}");
        }
        return null;
    }

    public async Task<AuthResult> UpdateProfileAsync(string name, string contact)
    {
        try
        {
            var payload = new { Name = name, Contact = contact };
            var response = await _http.PutAsJsonAsync("api/auth/me", payload);

            if (response.IsSuccessStatusCode)
            {
                UserName = name;
                await _localStorage.SetAsync("userName", name);
                return AuthResult.Ok();
            }

            var error = await response.Content.ReadAsStringAsync();
            return AuthResult.Fail(error ?? "Failed to update profile.");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Error: {ex.Message}");
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
                UserName = userMe?.Name;
                await _localStorage.SetAsync("userName", UserName ?? "");
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

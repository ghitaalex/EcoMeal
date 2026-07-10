using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace EcoMeal.Client.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedLocalStorage localStorage, ILogger<CustomAuthenticationStateProvider> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var tokenResult = await _localStorage.GetAsync<string>("authToken");
            var rolesResult = await _localStorage.GetAsync<List<string>>("userRoles");

            if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
            {
                return new AuthenticationState(_anonymous);
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, tokenResult.Value)
            }, "CustomAuth");

            if (rolesResult.Success && rolesResult.Value != null)
            {
                foreach (var role in rolesResult.Value)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve authentication state from local storage. This is expected during pre-rendering.");
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserAuthentication(string token, List<string> roles)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, token)
        }, "CustomAuth");

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var user = new ClaimsPrincipal(identity);
        var state = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(state);
    }

    public void NotifyUserLogout()
    {
        var state = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(state);
    }
}

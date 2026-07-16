using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace EcoMeal.Client.Services;

public class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly ProtectedLocalStorage _localStorage;

    public AuthenticationHeaderHandler(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _localStorage.GetAsync<string>("authToken");
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Value);
            }
        }
        catch (InvalidOperationException)
        {
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

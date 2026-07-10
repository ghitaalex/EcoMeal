using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace EcoMeal.Client.Services;

public class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ILogger<AuthenticationHeaderHandler> _logger;

    public AuthenticationHeaderHandler(ProtectedLocalStorage localStorage, ILogger<AuthenticationHeaderHandler> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to read authToken from local storage. This is expected during pre-rendering.");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

using EcoMeal.Client.Models;
using EcoMeal.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace EcoMeal.Client.Components.BusinessList;

public partial class BusinessList : IAsyncDisposable
{
    [Inject]
    public required BusinessService BusinessService { get; set; }

    [Inject]
    public required IJSRuntime JS { get; set; }

    private readonly string _mapElementId = $"business-map-{Guid.NewGuid():N}";
    private List<BusinessModel>? Businesses { get; set; }
    private IJSObjectReference? _mapModule;
    private UserLocationModel? _userLocation;
    private string? _selectedType;
    private string? _loadError;
    private string? _locationError;
    private string? _mapError;
    private string _viewMode = "list";
    private bool _isRetrying;
    private bool _isLocating;
    private bool _mapRenderRequested;

    private IEnumerable<string> BusinessTypes =>
        (Businesses ?? []).Select(b => b.BusinessTypeName).Distinct().OrderBy(t => t);

    private IEnumerable<BusinessModel> FavoriteBusinesses =>
        CategoryFilteredBusinesses.Where(b => b.IsFavorite);

    private IEnumerable<BusinessModel> CategoryFilteredBusinesses
    {
        get
        {
            IEnumerable<BusinessModel> results = Businesses ?? [];

            if (!string.IsNullOrWhiteSpace(_selectedType))
            {
                results = results.Where(b =>
                    b.BusinessTypeName.Equals(_selectedType, StringComparison.OrdinalIgnoreCase));
            }

            return results;
        }
    }

    private IEnumerable<BusinessModel> FilteredBusinesses =>
        CategoryFilteredBusinesses.Where(b => !b.IsFavorite);

    private IEnumerable<BusinessModel> MappableBusinesses =>
        CategoryFilteredBusinesses.Where(b => b.Latitude.HasValue && b.Longitude.HasValue);

    protected override async Task OnInitializedAsync()
    {
        await LoadBusinesses();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _mapModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/businessMap.js");
                await RequestLocationAsync();
            }
            catch (JSException)
            {
                _locationError = "Location services could not be started in this browser.";
                StateHasChanged();
            }
        }

        if (_mapRenderRequested && _viewMode == "map" && _mapModule is not null)
        {
            _mapRenderRequested = false;
            await RenderMapAsync();
        }
    }

    private async Task LoadBusinesses()
    {
        _loadError = null;
        _isRetrying = true;

        try
        {
            Businesses = await BusinessService.GetAllAsync();
            var favoriteIds = await BusinessService.GetFavoriteIdsAsync();
            foreach (var business in Businesses)
                business.IsFavorite = favoriteIds.Contains(business.Id);

            if (_userLocation is not null)
                await LoadDrivingDistances();

            _mapRenderRequested = _viewMode == "map";
        }
        catch
        {
            _loadError = "The EcoMeal API is not available right now.";
        }
        finally
        {
            _isRetrying = false;
        }
    }

    private async Task RequestLocationAsync()
    {
        if (_mapModule is null || _isLocating)
            return;

        _isLocating = true;
        _locationError = null;
        StateHasChanged();

        try
        {
            var result = await _mapModule.InvokeAsync<UserLocationModel>("getCurrentLocation");
            if (result.Success)
            {
                _userLocation = result;
                await LoadDrivingDistances();
                _mapRenderRequested = _viewMode == "map";
            }
            else
            {
                _locationError = result.Error ?? "Your location could not be determined.";
            }
        }
        catch (JSException)
        {
            _locationError = "Your location could not be determined.";
        }
        finally
        {
            _isLocating = false;
            StateHasChanged();
        }
    }

    private void SetViewMode(string viewMode)
    {
        _viewMode = viewMode;
        _mapError = null;
        _mapRenderRequested = viewMode == "map";
    }

    private async Task RenderMapAsync()
    {
        if (_mapModule is null)
            return;

        var markers = MappableBusinesses.Select(b => new MapBusinessMarkerModel
        {
            Id = b.Id,
            Name = b.Name,
            Address = b.Address,
            BusinessTypeName = b.BusinessTypeName,
            Latitude = b.Latitude!.Value,
            Longitude = b.Longitude!.Value,
            DistanceLabel = GetDistanceLabel(b)
        }).ToArray();

        try
        {
            await _mapModule.InvokeVoidAsync(
                "renderBusinessMap",
                _mapElementId,
                markers,
                _userLocation);
        }
        catch (JSException)
        {
            _mapError = "The map could not be loaded. Check your internet connection and try again.";
            StateHasChanged();
        }
    }

    private void SelectFilter(string? type)
    {
        _selectedType = type;
        _mapRenderRequested = _viewMode == "map";
    }

    private void HandleFavoriteChanged()
    {
        _mapRenderRequested = _viewMode == "map";
    }

    private double? GetApproximateDistance(BusinessModel business)
    {
        if (_userLocation is null || !business.Latitude.HasValue || !business.Longitude.HasValue)
            return null;

        const double earthRadiusKm = 6371;
        var latitudeDelta = DegreesToRadians(business.Latitude.Value - _userLocation.Latitude);
        var longitudeDelta = DegreesToRadians(business.Longitude.Value - _userLocation.Longitude);
        var startLatitude = DegreesToRadians(_userLocation.Latitude);
        var endLatitude = DegreesToRadians(business.Latitude.Value);
        var haversine = Math.Pow(Math.Sin(latitudeDelta / 2), 2) +
                        Math.Cos(startLatitude) * Math.Cos(endLatitude) *
                        Math.Pow(Math.Sin(longitudeDelta / 2), 2);

        return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
    }

    private string? GetDistanceLabel(BusinessModel business)
    {
        if (business.DistanceKm.HasValue && business.DurationMinutes.HasValue)
            return $"{business.DistanceKm.Value:0.0} km · {Math.Ceiling(business.DurationMinutes.Value):0} min drive";

        return business.DistanceKm switch
        {
            null => null,
            < 1 => $"About {Math.Round(business.DistanceKm.Value * 1000):0} m away",
            _ => $"About {business.DistanceKm.Value:0.0} km away"
        };
    }

    private async Task LoadDrivingDistances()
    {
        if (_userLocation is null || Businesses is null)
            return;

        foreach (var business in Businesses)
        {
            business.DistanceKm = GetApproximateDistance(business);
            business.DurationMinutes = null;
        }

        var distances = await BusinessService.GetDrivingDistances(
            _userLocation.Latitude,
            _userLocation.Longitude);

        foreach (var distance in distances)
        {
            var business = Businesses.FirstOrDefault(b => b.Id == distance.BusinessId);
            if (business is null)
                continue;

            business.DistanceKm = distance.DistanceKm;
            business.DurationMinutes = distance.DurationMinutes;
        }
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    private string GetIconForType(string type) => type.ToLowerInvariant() switch
    {
        "restaurant" => Icons.Material.Filled.Restaurant,
        "patiserie" => Icons.Material.Filled.BakeryDining,
        "cafe" => Icons.Material.Filled.Coffee,
        "supermarket" => Icons.Material.Filled.ShoppingCart,
        "bar" => Icons.Material.Filled.LocalBar,
        "pizza" => Icons.Material.Filled.LocalPizza,
        "fastfood" => Icons.Material.Filled.Fastfood,
        _ => Icons.Material.Filled.Store
    };

    public async ValueTask DisposeAsync()
    {
        if (_mapModule is null)
            return;

        try
        {
            await _mapModule.InvokeVoidAsync("disposeBusinessMap", _mapElementId);
            await _mapModule.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }

}

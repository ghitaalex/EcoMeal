using EcoMeal.Client.Models;
using EcoMeal.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace EcoMeal.Client.Components.SearchDropdown
{
    public partial class SearchDropdown : IDisposable
    {
        [Inject]
        public required BusinessService BusinessService { get; set; }

        [Inject]
        public required PackageService PackageService { get; set; }

        [Inject]
        public required NavigationManager Navigation { get; set; }

        private string _searchText = "";
        private bool _isOpen;
        private bool _loaded;
        private bool _loading;
        private readonly List<SearchResult> _allResults = new();

        private IEnumerable<SearchResult> FilteredResults =>
            string.IsNullOrWhiteSpace(_searchText)
                ? []
                : _allResults
                    .Where(r => r.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                        || (r.Description?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                        || r.Category.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                    .Take(20);

        protected override void OnInitialized()
        {
            Navigation.LocationChanged += OnLocationChanged;
        }

        private async Task OnInput(ChangeEventArgs e)
        {
            _searchText = e.Value?.ToString() ?? "";

            if (!_loaded && !_loading && !string.IsNullOrWhiteSpace(_searchText))
            {
                _loading = true;
                _isOpen = true;
                StateHasChanged();
                await LoadData();
                _loading = false;
            }

            _isOpen = !string.IsNullOrWhiteSpace(_searchText);
        }

        private void HandleFocus()
        {
            if (!string.IsNullOrWhiteSpace(_searchText) && _loaded)
                _isOpen = true;
        }

        private void HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Escape")
            {
                _isOpen = false;
                _searchText = "";
            }
        }

        private async Task LoadData()
        {
            var businesses = await BusinessService.GetAllAsync();
            foreach (var b in businesses)
            {
                _allResults.Add(new SearchResult
                {
                    ResultType = "Business",
                    Name = b.Name,
                    Description = b.Description,
                    ImageUrl = b.BusinessImageUrl,
                    Category = b.BusinessTypeName,
                    NavigateUrl = $"/business/{b.Id}",
                    ExtraInfo = b.Address
                });
            }

            _loaded = true;
            StateHasChanged();

            foreach (var b in businesses)
            {
                var packages = await PackageService.GetByBusinessId(b.Id);
                foreach (var p in packages)
                {
                    _allResults.Add(new SearchResult
                    {
                        ResultType = "Package",
                        Name = p.Name,
                        Description = p.Description,
                        ImageUrl = p.PackageImageUrl,
                        Category = p.PackageTypeName,
                        NavigateUrl = $"/business/{b.Id}",
                        ExtraInfo = p.Price.ToString("0.00") + " RON"
                    });
                }
            }
            StateHasChanged();
        }

        private void Navigate(string url)
        {
            _searchText = "";
            _isOpen = false;
            Navigation.NavigateTo(url);
        }

        private void Close()
        {
            _isOpen = false;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            _isOpen = false;
            _searchText = "";
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
        }
    }
}

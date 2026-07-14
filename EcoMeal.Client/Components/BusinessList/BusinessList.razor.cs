using EcoMeal.Client.Models;
using EcoMeal.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace EcoMeal.Client.Components.BusinessList
{
    public partial class BusinessList
    {
        [Inject]
        public required BusinessService BusinessService { get; set; }

        private List<BusinessModel>? Businesses { get; set; }
        private string? _selectedType;

        private IEnumerable<string> BusinessTypes =>
            (Businesses ?? []).Select(b => b.BusinessTypeName).Distinct().OrderBy(t => t);

        private IEnumerable<BusinessModel> FilteredBusinesses
        {
            get
            {
                var results = Businesses ?? [];
                if (!string.IsNullOrWhiteSpace(_selectedType))
                    results = results.Where(b => b.BusinessTypeName.Equals(_selectedType, StringComparison.OrdinalIgnoreCase)).ToList();
                return results;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadBusinesses();
        }

        private async Task LoadBusinesses()
        {
            Businesses = await BusinessService.GetAllAsync();
        }

        private void SelectFilter(string? type)
        {
            _selectedType = type;
        }

        private Color GetChipColor(string? type) =>
            _selectedType == type ? Color.Success : Color.Default;

        private string GetChipStyle(string? type) =>
            _selectedType == type
                ? "background: linear-gradient(135deg, #059669, #059669); color: white; font-weight: 600;"
                : "background: var(--bg-surface); color: var(--accent-text); border: 1.5px solid var(--border-color); font-weight: 500;";

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
    }
}

using EcoMeal.Client.Models;
using EcoMeal.Client.Services;
using Microsoft.AspNetCore.Components;

namespace EcoMeal.Client.Components.BusinessList
{
    public partial class BusinessList
    {
        [Inject]
        public required BusinessService BusinessService { get; set; }

        [CascadingParameter(Name = "SearchText")]
        public string SearchText { get; set; } = string.Empty;

        private List<BusinessModel>? Businesses { get; set; }

        private IEnumerable<BusinessModel> FilteredBusinesses =>
            string.IsNullOrWhiteSpace(SearchText)
                ? Businesses ?? []
                : (Businesses ?? []).Where(b =>
                    b.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        protected override async Task OnInitializedAsync()
        {
            await LoadBusinesses();
        }

        private async Task LoadBusinesses()
        {
            Businesses = await BusinessService.GetAllAsync();
        }
    }
}

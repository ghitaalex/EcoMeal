using EcoMeal.Client.Models;
using EcoMeal.Client.Services;
using Microsoft.AspNetCore.Components;

namespace EcoMeal.Client.Components.BusinessList
{
    public partial class BusinessList
    {
        [Inject]
        public required BusinessService BusinessService { get; set; }
        private List<BusinessModel>? Businesses { get; set; }

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

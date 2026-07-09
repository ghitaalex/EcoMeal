using EcoMeal.Client.Models;
using EcoMeal.Client.Services;
using Microsoft.AspNetCore.Components;

namespace EcoMeal.Client.Components.BusinessCard
{
    public partial class BusinessCard
    {
        [Parameter]
        public required BusinessModel Business { get; set; }

        [Inject]
        public required BusinessService BusinessService { get; set; }
        [Inject]
        public required PackageService PackageService { get; set; }
        [Inject]
        public required NavigationManager Navigation { get; set; }

        private bool _deleted;
        private decimal? _lowestPrice;
        private string? _earliestPickup;

        protected override async Task OnInitializedAsync()
        {
            var packages = await PackageService.GetByBusinessId(Business.Id);
            if (packages.Count > 0)
            {
                _lowestPrice = packages.Min(p => p.Price);
                var earliest = packages.OrderBy(p => p.PickUpStart.TimeOfDay).First();
                _earliestPickup = earliest.PickUpStart.ToString("HH:mm");
            }
        }

        private async Task HandleDelete()
        {
            var success = await BusinessService.DeleteAsync(Business.Id);

            if (success)
            {
                _deleted = true;
            }
        }

        public void NavigateToDetails()
        {
            Navigation.NavigateTo($"business/{Business.Id}");
        }

        public void NavigateToEdit()
        {
            Navigation.NavigateTo($"business/{Business.Id}/edit");
        }
    }
}

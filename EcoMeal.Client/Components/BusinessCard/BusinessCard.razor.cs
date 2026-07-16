using EcoMeal.Client.Models;
using EcoMeal.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace EcoMeal.Client.Components.BusinessCard
{
    public partial class BusinessCard
    {
        [Parameter]
        public required BusinessModel Business { get; set; }

        [Parameter]
        public EventCallback OnFavoriteChanged { get; set; }

        [Inject]
        public required BusinessService BusinessService { get; set; }
        [Inject]
        public required PackageService PackageService { get; set; }
        [Inject]
        public required NavigationManager Navigation { get; set; }
        [Inject]
        public required ISnackbar Snackbar { get; set; }

        private MudMessageBox? _deleteConfirmBox;
        private bool _deleted;
        private bool _deleting;
        private bool _updatingFavorite;
        private decimal? _lowestPrice;
        private string? _earliestPickup;

        protected override async Task OnInitializedAsync()
        {
            var packages = await PackageService.GetByBusinessId(Business.Id);
            if (packages.Count > 0)
            {
                _lowestPrice = packages.Min(p => p.Price);
                var earliest = packages.OrderBy(p => p.PickUpStart.ToLocalTime().TimeOfDay).First();
                _earliestPickup = earliest.PickUpStart.ToLocalTime().ToString("HH:mm");
            }
        }

        private async Task HandleDelete()
        {
            if (_deleteConfirmBox is null)
                return;

            var result = await _deleteConfirmBox.ShowAsync();
            if (result != true)
                return;

            _deleting = true;
            StateHasChanged();

            var (success, errorMessage) = await BusinessService.DeleteAsync(Business.Id);

            if (success)
            {
                Snackbar.Add($"\"{Business.Name}\" has been deleted.", Severity.Success);
                _deleted = true;
            }
            else
            {
                Snackbar.Add(errorMessage ?? "Failed to delete business.", Severity.Error);
            }

            _deleting = false;
        }

        private async Task ToggleFavorite()
        {
            _updatingFavorite = true;

            var success = Business.IsFavorite
                ? await BusinessService.RemoveFavoriteAsync(Business.Id)
                : await BusinessService.AddFavoriteAsync(Business.Id);

            if (success)
            {
                Business.IsFavorite = !Business.IsFavorite;
                await OnFavoriteChanged.InvokeAsync();
            }
            else
            {
                Snackbar.Add("Could not update favorites.", Severity.Error);
            }

            _updatingFavorite = false;
        }

        public void NavigateToDetails()
        {
            Navigation.NavigateTo($"business/{Business.Id}");
        }

        public void NavigateToEdit()
        {
            Navigation.NavigateTo($"business/{Business.Id}/edit");
        }

        private string FormattedDistance
        {
            get
            {
                if (Business.DistanceKm is null)
                    return string.Empty;

                var distance = Business.DistanceKm < 1
                    ? $"{Math.Round(Business.DistanceKm.Value * 1000):0} m"
                    : $"{Business.DistanceKm.Value:0.0} km";

                return Business.DurationMinutes.HasValue
                    ? $"{distance} · {Math.Ceiling(Business.DurationMinutes.Value):0} min"
                    : $"About {distance}";
            }
        }
    }
}

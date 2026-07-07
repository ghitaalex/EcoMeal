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

        private bool _deleted;

        private async Task HandleDelete()
        {
            var success = await BusinessService.DeleteAsync(Business.Id);

            if (success)
            {
                _deleted = true;
            }
        }
    }
}

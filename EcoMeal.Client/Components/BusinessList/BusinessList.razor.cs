using EcoMeal.Client.Models;
using EcoMeal.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcoMeal.Client.Components.BusinessList
{
    public partial class BusinessList
    {
        [Inject]
        public required BusinessService BusinessService { get; set; }
        private List<BusinessModel>? Businesses { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Businesses = await BusinessService.GetAllAsync();
        }
    }
}

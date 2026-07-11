using EcoMeal.Client.Models;
using Microsoft.AspNetCore.Components;

namespace EcoMeal.Client.Components.SearchResultCard
{
    public partial class SearchResultCard
    {
        [Parameter]
        public required SearchResult Result { get; set; }

        [Parameter]
        public EventCallback OnClick { get; set; }

        private string GetImageStyle()
        {
            if (!string.IsNullOrEmpty(Result.ImageUrl))
                return $"background-image: url('{Result.ImageUrl}');";
            return "background: linear-gradient(135deg, #0B0F19 0%, #047857 100%);";
        }

        private string GetIcon() => Result.ResultType == "Business" ? "bi bi-shop" : "bi bi-box-seam";
    }
}

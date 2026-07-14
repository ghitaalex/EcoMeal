using EcoMeal.Client.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace EcoMeal.Client.Services
{
    public class PackageService
    {
        private readonly HttpClient _http;
        public PackageService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PackageGetModel>> GetByBusinessId(int businessId)
        {
            var packages = await _http.GetFromJsonAsync<List<PackageGetModel>>($"api/business/{businessId}/package");
            return packages ?? new List<PackageGetModel>();
        }

        public async Task<PackageGetModel?> GetById(int businessId, int packageId)
        {
            var packages = await GetByBusinessId(businessId);
            return packages.FirstOrDefault(p => p.Id == packageId);
        }

        public async Task AddAsync(int businessId, PackageAddModel package, IBrowserFile? image = null)
        {
            var content = CreateMultipartContent(package, image);
            await _http.PostAsync($"api/business/{businessId}/package", content);
        }

        public async Task<bool> EditAsync(int businessId, int packageId, PackageAddModel package, IBrowserFile? image = null)
        {
            var content = CreateMultipartContent(package, image);
            var response = await _http.PutAsync($"api/business/{businessId}/package/{packageId}", content);
            return response.IsSuccessStatusCode;
        }

        private MultipartFormDataContent CreateMultipartContent(PackageAddModel package, IBrowserFile? image)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(package.Name), "Name");
            content.Add(new StringContent(package.Description), "Description");
            content.Add(new StringContent(package.Price.ToString()), "Price");
            content.Add(new StringContent(package.NoPackages.ToString()), "NoPackages");
            content.Add(new StringContent(package.StartPickup.ToString("o")), "StartPickup");
            content.Add(new StringContent(package.EndPickup.ToString("o")), "EndPickup");
            content.Add(new StringContent(package.PackageTypeId.ToString()), "PackageTypeId");

            if (image is not null)
            {
                var stream = image.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
                content.Add(new StreamContent(stream), "PackageImage", image.Name);
            }

            return content;
        }

        public async Task<bool> DeleteAsync(int businessId, int packageId)
        {
            var response = await _http.DeleteAsync($"api/business/{businessId}/package/{packageId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<PackageTypeModel>> GetPackageTypes()
        {
            var types = await _http.GetFromJsonAsync<List<PackageTypeModel>>("api/PackageType");
            return types ?? new List<PackageTypeModel>();
        }
    }
}

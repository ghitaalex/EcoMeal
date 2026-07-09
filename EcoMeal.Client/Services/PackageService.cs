using EcoMeal.Client.Models;

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

        public async Task AddAsync(int businessId, PackageAddModel package)
        {
            await _http.PostAsJsonAsync($"api/business/{businessId}/package", package);
        }

        public async Task<bool> EditAsync(int businessId, int packageId, PackageAddModel package)
        {
            var response = await _http.PutAsJsonAsync($"api/business/{businessId}/package/{packageId}", package);
            return response.IsSuccessStatusCode;
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

using EcoMeal.Client.Models;

namespace EcoMeal.Client.Services
{
    public class BusinessService
    {
        private readonly HttpClient _http;
        public BusinessService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BusinessModel>> GetAllAsync()
        {
            var businesses = await _http.GetFromJsonAsync<List<BusinessModel>>("/api/business");
            return businesses ?? new List<BusinessModel>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/business/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<BusinessDetailsModel?> GetOneById(int id)
        {
            var business = await _http.GetFromJsonAsync<BusinessDetailsModel>($"api/business/{id}");
            return business;
        }

        public async Task AddAsync(BusinessAddModel business)
        {
            await _http.PostAsJsonAsync("api/business", business);
        }

        public async Task<bool> EditAsync(int id, BusinessAddModel business)
        {
            var response = await _http.PutAsJsonAsync($"api/business/{id}", business);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<BusinessTypeModel>> GetBusinessTypes()
        {
            var types = await _http.GetFromJsonAsync<List<BusinessTypeModel>>("api/BusinessType");
            return types ?? new List<BusinessTypeModel>();
        }
    }
}

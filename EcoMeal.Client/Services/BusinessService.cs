using EcoMeal.Client.Models;
using Microsoft.AspNetCore.Components.Forms;

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

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/business/{id}");
            if (response.IsSuccessStatusCode)
                return (true, null);

            var body = await response.Content.ReadAsStringAsync();
            return (false, $"Delete failed ({(int)response.StatusCode}): {body}");
        }

        public async Task<BusinessDetailsModel?> GetOneById(int id)
        {
            var business = await _http.GetFromJsonAsync<BusinessDetailsModel>($"api/business/{id}");
            return business;
        }

        public async Task AddAsync(BusinessAddModel business, IBrowserFile? image = null)
        {
            var content = CreateMultipartContent(business, image);
            await _http.PostAsync("api/business", content);
        }

        public async Task<bool> EditAsync(int id, BusinessAddModel business, IBrowserFile? image = null)
        {
            var content = CreateMultipartContent(business, image);
            var response = await _http.PutAsync($"api/business/{id}", content);
            return response.IsSuccessStatusCode;
        }

        private MultipartFormDataContent CreateMultipartContent(BusinessAddModel business, IBrowserFile? image)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(business.Name), "Name");
            content.Add(new StringContent(business.Address), "Address");
            if (business.Description is not null)
                content.Add(new StringContent(business.Description), "Description");
            content.Add(new StringContent(business.Contact), "Contact");
            content.Add(new StringContent(business.BusinessTypeId.ToString()), "BusinessTypeId");

            if (image is not null)
            {
                var stream = image.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
                content.Add(new StreamContent(stream), "BusinessImage", image.Name);
            }

            return content;
        }

        public async Task<List<BusinessTypeModel>> GetBusinessTypes()
        {
            var types = await _http.GetFromJsonAsync<List<BusinessTypeModel>>("api/BusinessType");
            return types ?? new List<BusinessTypeModel>();
        }
    }
}

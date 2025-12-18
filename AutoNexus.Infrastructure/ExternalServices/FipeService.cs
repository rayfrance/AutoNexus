using AutoNexus.Application.DTOs.Fipe;
using AutoNexus.Application.Interfaces;
using System.Text.Json;

namespace AutoNexus.Infrastructure.ExternalServices
{
    public class FipeService : IFipeService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };

        public FipeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<FipeReferenceResponse>> GetBrandsAsync()
        {   
            HttpResponseMessage response = await _httpClient.GetAsync("carros/marcas");
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<FipeReferenceResponse>>(content, _jsonOptions)
                   ?? new List<FipeReferenceResponse>();
        }

        public async Task<IEnumerable<FipeReferenceResponse>> GetModelsAsync(string brandId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"carros/marcas/{brandId}/modelos");
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<FipeModelResponse>(content, _jsonOptions);

            return result?.Models ?? new List<FipeReferenceResponse>();
        }

        public async Task<IEnumerable<FipeReferenceResponse>> GetYearsAsync(string brandId, string modelId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"carros/marcas/{brandId}/modelos/{modelId}/anos");
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<FipeReferenceResponse>>(content, _jsonOptions)
                   ?? new List<FipeReferenceResponse>();
        }

        public async Task<FipeVehicleResponse?> GetVehicleDetailsAsync(string brandId, string modelId, string yearId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"carros/marcas/{brandId}/modelos/{modelId}/anos/{yearId}");

            if (!response.IsSuccessStatusCode) return null;

            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FipeVehicleResponse>(content, _jsonOptions);
        }
    }
}
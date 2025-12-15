using System.Net.Http.Json;
using AutoNexus.Application.DTOs.Fipe;
using AutoNexus.Application.Interfaces;
using AutoNexus.Domain;

namespace AutoNexus.Infrastructure.ExternalServices
{
    public class FipeService : IFipeService
    {
        private readonly HttpClient _httpClient;
        public FipeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<FipeReferenceResponse>> GetBrandsAsync()
        {
            // GET: https://parallelum.com.br/fipe/api/v1/carros/marcas
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<FipeReferenceResponse>>($"{Constants.FIPE_URL}/marcas");

            return response ?? Enumerable.Empty<FipeReferenceResponse>();
        }

        public async Task<IEnumerable<FipeReferenceResponse>> GetModelsAsync(string brandId)
        {
            // GET: https://parallelum.com.br/fipe/api/v1/carros/marcas/{id}/modelos
            var response = await _httpClient.GetFromJsonAsync<FipeModelResponse>($"{Constants.FIPE_URL}/marcas/{brandId}/modelos");

            return response?.Modelos ?? Enumerable.Empty<FipeReferenceResponse>();
        }
    }
}
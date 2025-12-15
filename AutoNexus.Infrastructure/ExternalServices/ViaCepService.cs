using System.Net.Http.Json;
using AutoNexus.Application.DTOs.Address;
using AutoNexus.Application.Interfaces;
using AutoNexus.Domain;
using static System.Net.Mime.MediaTypeNames;

namespace AutoNexus.Infrastructure.ExternalServices
{
    public class ViaCepService : IAddressService
    {
        private readonly HttpClient _httpClient;

        public ViaCepService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ViaCepResponse?> GetAddressByZipCodeAsync(string zipCode)
        {
            string cleanZipCode = GetOnlyNumbers(zipCode);
            if (cleanZipCode.Length != 8)
                return null;

            try
            {
                var response = await _httpClient.GetFromJsonAsync<ViaCepResponse>($"{Constants.VIACEP_URL}/{cleanZipCode}/json/");

                if (response != null && response.Error)
                    return null;

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetOnlyNumbers(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("-", "").Replace(".", "").Trim();
        }
    }
}
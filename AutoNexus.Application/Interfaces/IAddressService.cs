using AutoNexus.Application.DTOs.Address;

namespace AutoNexus.Application.Interfaces
{
    public interface IAddressService
    {
        Task<ViaCepResponse?> GetAddressByZipCodeAsync(string zipCode);
    }
}
using AutoNexus.Application.DTOs.Fipe;

namespace AutoNexus.Application.Interfaces
{
    public interface IFipeService
    {
        Task<IEnumerable<FipeReferenceResponse>> GetBrandsAsync();
        Task<IEnumerable<FipeReferenceResponse>> GetModelsAsync(string brandId);
    }
}
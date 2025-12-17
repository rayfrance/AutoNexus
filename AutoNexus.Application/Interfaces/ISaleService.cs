using AutoNexus.Application.DTOs.Sales;
using AutoNexus.Domain.Entities;

namespace AutoNexus.Application.Interfaces
{
    public interface ISaleService
    {
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();

        Task ProcessSaleAsync(CreateSaleDto dto);
    }
}
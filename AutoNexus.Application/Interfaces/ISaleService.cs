using AutoNexus.Application.Common; 
using AutoNexus.Application.DTOs.Sales;
using AutoNexus.Domain.Entities;

namespace AutoNexus.Application.Interfaces
{
    public interface ISaleService
    {
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
        Task ProcessSaleAsync(CreateSaleDto dto);
        Task<Client?> GetClientByCpfAsync(string cpf);
        Task<PaginatedList<Sale>> SearchSalesAsync(string searchString, int pageNumber, int pageSize);
    }
}
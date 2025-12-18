using AutoNexus.Application.Common;
using AutoNexus.Application.Common.Validation;
using AutoNexus.Application.DTOs.Sales;
using AutoNexus.Application.Interfaces;
using AutoNexus.Domain.Entities;
using AutoNexus.Domain.Enums;
using AutoNexus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoNexus.Infrastructure.Services
{
    public class SaleService : ISaleService
    {
        private readonly ApplicationDbContext _context;

        public SaleService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Public Methods (Interface Implementation)

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Where(v => v.Status == VehicleStatus.Available && !v.IsDeleted)
                .OrderBy(v => v.Model)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sale>> GetAllSalesAsync()
        {
            return await _context.Sales
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Vehicle)
                .ThenInclude(v => v.Manufacturer)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<PaginatedList<Sale>> SearchSalesAsync(string searchString, int pageNumber, int pageSize)
        {
            var query = _context.Sales
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Vehicle)
                .ThenInclude(v => v.Manufacturer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    s.ProtocolNumber.Contains(searchString) ||
                    s.Client.Name.Contains(searchString) ||
                    s.Vehicle.Model.Contains(searchString)
                );
            }
            query = query.OrderByDescending(s => s.CreatedAt);

            return await PaginatedList<Sale>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<Client?> GetClientByCpfAsync(string cpf)
        {
            var cleanCpf = CpfValidator.GetOnlyNumbers(cpf);
            return await _context.Clients.FirstOrDefaultAsync(c => c.CPF == cleanCpf);
        }

        public async Task ProcessSaleAsync(CreateSaleDto dto)
        {
            dto.ClientCPF = CpfValidator.GetOnlyNumbers(dto.ClientCPF);

            var vehicle = await ValidateAndGetVehicleAsync(dto);
            ValidateSaleRules(dto);
            var client = await GetOrCreateClientAsync(dto);

            await ExecuteSaleTransactionAsync(vehicle, client, dto);
        }

        #endregion

        #region Private Helper Methods

        private async Task<Vehicle> ValidateAndGetVehicleAsync(CreateSaleDto dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);

            if (vehicle == null)
                throw new InvalidOperationException("Veículo não encontrado.");

            if (vehicle.Status != VehicleStatus.Available)
                throw new InvalidOperationException($"O veículo '{vehicle.Model}' não está disponível para venda (Status: {vehicle.Status}).");

            if (dto.SalePrice > vehicle.Price)
                throw new InvalidOperationException($"O preço de venda (R$ {dto.SalePrice:N2}) excede a tabela (R$ {vehicle.Price:N2}).");

            return vehicle;
        }

        private void ValidateSaleRules(CreateSaleDto dto)
        {
            if (dto.SaleDate.Date > DateTime.Now.Date)
                throw new InvalidOperationException("A data da venda não pode ser futura.");
        }

        private async Task<Client> GetOrCreateClientAsync(CreateSaleDto dto)
        {
            var cleanCpf = CpfValidator.GetOnlyNumbers(dto.ClientCPF);

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.CPF == cleanCpf);

            if (client == null)
            {
                client = await CreateClient(dto, cleanCpf);
            }
            else
            {
                client.Name = dto.ClientName;
                client.Phone = dto.ClientPhone;
                _context.Clients.Update(client);
            }

            return client;
        }

        private async Task<Client> CreateClient(CreateSaleDto dto, string cleanCpf)
        {
            Client client = new Client
            {
                Name = dto.ClientName,
                CPF = cleanCpf,
                Phone = dto.ClientPhone,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Clients.AddAsync(client);
            return client;
        }

        private async Task ExecuteSaleTransactionAsync(Vehicle vehicle, Client client, CreateSaleDto dto)
        {
            var sale = new Sale
            {
                Vehicle = vehicle,
                Client = client,
                SaleDate = dto.SaleDate,
                SalePrice = dto.SalePrice,
                ProtocolNumber = GenerateUniqueProtocol(),
                CreatedAt = DateTime.UtcNow
            };

            vehicle.Status = VehicleStatus.Sold;
            _context.Vehicles.Update(vehicle);

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
        }

        private string GenerateUniqueProtocol()
        {
            return $"{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        }

        #endregion
    }
}
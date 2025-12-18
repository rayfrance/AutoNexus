using Microsoft.EntityFrameworkCore;
using AutoNexus.Infrastructure.Data;
using AutoNexus.Infrastructure.Services;
using AutoNexus.Domain.Entities;
using AutoNexus.Domain.Enums;
using AutoNexus.Application.DTOs.Sales;

namespace AutoNexus.Tests.Services
{
    public class SaleServiceTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact(DisplayName = "GIVEN a non-existent vehicle " +
            "WHEN processing a sale " +
            "SHOULD throw InvalidOperationException")]
        public async Task ProcessSale_NonExistentVehicle_ThrowsException()
        {
            var context = GetInMemoryContext();
            var service = new SaleService(context);
            var dto = new CreateSaleDto { VehicleId = 99 };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessSaleAsync(dto));
            Assert.Equal("Veículo não encontrado.", ex.Message);
        }

        [Fact(DisplayName = "GIVEN a sold vehicle " +
            "WHEN processing a sale " +
            "SHOULD throw InvalidOperationException")]
        public async Task ProcessSale_SoldVehicle_ThrowsException()
        {
            var context = GetInMemoryContext();
            var vehicle = new Vehicle { Id = 1, Model = "Civic", Status = VehicleStatus.Sold, Price = 50000 };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();

            var service = new SaleService(context);
            var dto = new CreateSaleDto { VehicleId = 1, SalePrice = 45000 };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessSaleAsync(dto));
            Assert.Contains("não está disponível", ex.Message);
        }

        [Fact(DisplayName = "GIVEN a sale price higher than table " +
            "WHEN processing " +
            "SHOULD throw InvalidOperationException")]
        public async Task ProcessSale_HighPrice_ThrowsException()
        {
            var context = GetInMemoryContext();
            var vehicle = new Vehicle { Id = 1, Price = 50000, Status = VehicleStatus.Available };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();

            var service = new SaleService(context);
            var dto = new CreateSaleDto { VehicleId = 1, SalePrice = 60000 };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessSaleAsync(dto));
            Assert.Contains("excede a tabela", ex.Message);
        }

        [Fact(DisplayName = "GIVEN a future date " +
            "WHEN processing a sale " +
            "SHOULD throw InvalidOperationException")]
        public async Task ProcessSale_FutureDate_ThrowsException()
        {
            var context = GetInMemoryContext();
            var vehicle = new Vehicle { Id = 1, Price = 50000, Status = VehicleStatus.Available };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();

            var service = new SaleService(context);
            var dto = new CreateSaleDto { VehicleId = 1, SalePrice = 40000, SaleDate = DateTime.Now.AddDays(1) };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessSaleAsync(dto));
            Assert.Equal("A data da venda não pode ser futura.", ex.Message);
        }

        [Fact(DisplayName = "GIVEN a new client CPF " +
            "WHEN processing a sale " +
            "SHOULD create client and register sale")]
        public async Task ProcessSale_NewClient_RegistersSuccessfully()
        {
            var context = GetInMemoryContext();
            var vehicle = new Vehicle { Id = 1, Price = 100000, Status = VehicleStatus.Available };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();

            var service = new SaleService(context);
            var dto = new CreateSaleDto
            {
                VehicleId = 1,
                SalePrice = 95000,
                SaleDate = DateTime.Now,
                ClientCPF = "12345678901",
                ClientName = "Test Client"
            };

            await service.ProcessSaleAsync(dto);

            var sale = await context.Sales.AnyAsync(s => s.VehicleId == 1);
            var clientCreated = await context.Clients.AnyAsync(c => c.CPF == "12345678901");

            Assert.True(sale);
            Assert.True(clientCreated);
            Assert.Equal(VehicleStatus.Sold, vehicle.Status);
        }

        [Fact(DisplayName = "GIVEN a search string " +
            "WHEN searching sales " +
            "SHOULD return filtered and paginated results")]
        public async Task SearchSales_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Criar objetos completos para garantir que o Include funcione no Search
            var manufacturer = new Manufacturer { Id = 1, Name = "Toyota" };
            var vehicle = new Vehicle { Id = 1, Model = "Corolla", Manufacturer = manufacturer, Price = 100000 };
            var client = new Client { Id = 1, Name = "Ayrton Senna", CPF = "11122233344" };

            context.Manufacturers.Add(manufacturer);
            context.Vehicles.Add(vehicle);
            context.Clients.Add(client);

            var sale = new Sale
            {
                ProtocolNumber = "ANX-999",
                Client = client,
                Vehicle = vehicle,
                SaleDate = DateTime.Now
            };

            context.Sales.Add(sale);
            await context.SaveChangesAsync();

            var service = new SaleService(context);

            // Act - Testando busca pelo Nome do Cliente (cobre mais linhas do serviço)
            var result = await service.SearchSalesAsync("Ayrton", 1, 10);

            // Assert
            Assert.NotEmpty(result); // Garante que não está vazio
            Assert.Equal("ANX-999", result.First().ProtocolNumber);
            Assert.Equal("Ayrton Senna", result.First().Client.Name);
        }

        [Fact(DisplayName = "GIVEN a CPF with formatting " +
            "WHEN fetching client " +
            "SHOULD return clean CPF record")]
        public async Task GetClientByCpf_FormattedCpf_ReturnsCorrectClient()
        {
            // Arrange
            var context = GetInMemoryContext();
            context.Clients.Add(new Client { Name = "Jose", CPF = "12345678901", Phone = "00" });
            await context.SaveChangesAsync();

            var service = new SaleService(context);

            // Act
            var result = await service.GetClientByCpfAsync("123.456.789-01");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("12345678901", result.CPF);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AutoNexus.Infrastructure.Data;
using AutoNexus.Domain.Enums;

namespace AutoNexus.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public int TotalVehiclesAvailable { get; set; }
        public decimal TotalStockValue { get; set; }
        public int ActiveCustomersCount { get; set; }
        public int SalesThisMonthCount { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public List<string> ManufacturerLabels { get; set; } = new();
        public List<int> VehicleCounts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            await GetKpiData(firstDayOfMonth);

            await GetChartData(firstDayOfMonth);

            return Page();
        }

        private async Task GetKpiData(DateTime firstDayOfMonth)
        {
            // 1. Veículos disponíveis no pátio
            TotalVehiclesAvailable = await _context.Vehicles
                .Where(v => !v.IsDeleted)
                .CountAsync(v => v.Status == VehicleStatus.Available);

            // 2. Soma do preço de todos os veículos disponíveis (Valor em estoque)
            TotalStockValue = await _context.Vehicles
                .Where(v => v.Status == VehicleStatus.Available && !v.IsDeleted)
                .SumAsync(v => v.Price);

            // 3. Quantidade de vendas realizadas no mês atual
            SalesThisMonthCount = await _context.Sales
                .CountAsync(s => s.SaleDate >= firstDayOfMonth);

            // 4. Soma do faturamento das vendas deste mês
            RevenueThisMonth = await _context.Sales
                .Where(s => s.SaleDate >= firstDayOfMonth)
                .SumAsync(s => s.SalePrice);

            // 5. Contagem de clientes únicos
            ActiveCustomersCount = await _context.Sales
                .Select(s => s.ClientId)
                .Distinct()
                .CountAsync();
        }

        private async Task GetChartData(DateTime firstDayOfMonth)
        {
            var stockByManufacturer = await _context.Vehicles
                .Where(v => v.Status == VehicleStatus.Available && !v.IsDeleted)
                .Include(v => v.Manufacturer)
                .GroupBy(v => v.Manufacturer.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ManufacturerLabels = stockByManufacturer.Select(x => x.Name).ToList();
            VehicleCounts = stockByManufacturer.Select(x => x.Count).ToList();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AutoNexus.Infrastructure.Data;
using AutoNexus.Domain.Enums;
using System.Text;


namespace AutoNexus.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly GeminiService _geminiService;
        public int TotalVehiclesAvailable { get; set; }
        public decimal TotalStockValue { get; set; }
        public int ActiveCustomersCount { get; set; }
        public int SalesThisMonthCount { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public List<string> ManufacturerLabels { get; set; } = new();
        public List<int> VehicleCounts { get; set; } = new();
        public string AiInsights { get; set; } = string.Empty;
        public List<Domain.Entities.Sale> RecentSales { get; set; } = new();

        public IndexModel(ApplicationDbContext context, GeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            await LoadAllDashboardData();

            return Page();
        }
        private async Task LoadAllDashboardData()
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            await GetKpiData(firstDayOfMonth);

            await GetChartData(firstDayOfMonth);

            await GetRecentSales();
        }

        private async Task GetRecentSales()
        {
            RecentSales = await _context.Sales
                .Include(s => s.Vehicle)
                .Include(s => s.Client)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToListAsync();
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


        public async Task<IActionResult> OnPostGenerateInsightsAsync()
        {
            await LoadAllDashboardData();

            var reportData = new
            {
                CurrentRevenue = RevenueThisMonth,
                TotalSales = SalesThisMonthCount,
                StockValue = TotalStockValue,
                AvailableCars = TotalVehiclesAvailable,
                TopManufacturers = ManufacturerLabels
            };

            try
            {
                string jsonData = System.Text.Json.JsonSerializer.Serialize(reportData);
                AiInsights = await _geminiService.GetVendasInsightsAsync(jsonData, ManufacturerLabels);
            }
            catch (Exception ex)
            {
                AiInsights = "Erro ao processar: " + ex.Message;
            }

            return Page();
        }
        public async Task<IActionResult> OnGetExportSalesCsvAsync()
        {
            var sales = await _context.Sales
                .Include(s => s.Vehicle)
                .Include(s => s.Client)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Numero de Protocolo;Data;Cliente;CPF;Veiculo;Valor Vendido");

            foreach (var sale in sales)
            {
                builder.AppendLine($"{sale.ProtocolNumber};{sale.SaleDate:dd/MM/yyyy};{sale.Client.Name};{sale.Client.CPF};{sale.Vehicle.Model};{sale.SalePrice:F2}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(csvBytes, "text/csv", $"Relatorio_Vendas_AutoNexus_{DateTime.Now.Date:dd_MM_yyyy}.csv");
        }
    }
}
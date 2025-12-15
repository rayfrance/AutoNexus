using AutoNexus.Application.Common;
using AutoNexus.Domain.Entities;
using AutoNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoNexus.Web.Controllers
{
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Vehicle
        // Parâmetros novos: searchString (Filtro) e pageNumber (Paginação)
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            // 1. Mantém o filtro na barra de busca ao mudar de página
            ViewData["CurrentFilter"] = searchString;

            // 2. Query Base (Lazy Loading ainda não executou)
            var vehicles = _context.Vehicles
                .Include(v => v.Manufacturer)
                .AsNoTracking(); // Requisito ANX-10 (Performance: apenas leitura)

            // 3. Aplica Filtro se houver texto
            if (!string.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(s => s.Model.Contains(searchString)
                                            || s.Manufacturer.Name.Contains(searchString));
            }

            // 4. Ordenação Padrão (Mais novos primeiro)
            vehicles = vehicles.OrderByDescending(v => v.CreatedAt);

            // 5. Paginação (Definimos 5 itens por página para testar fácil)
            int pageSize = 5;

            // Retorna a lista paginada para a View
            return View(await PaginatedList<Vehicle>.CreateAsync(vehicles, pageNumber ?? 1, pageSize));
        }
    }
}
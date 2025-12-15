using AutoNexus.Application.Common;
using AutoNexus.Application.Interfaces;
using AutoNexus.Application.DTOs.Fipe;
using AutoNexus.Domain.Entities;
using AutoNexus.Domain.Enums;
using AutoNexus.Infrastructure.Data;
using AutoNexus.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoNexus.Web.Controllers
{
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFipeService _fipeService;

        public VehicleController(ApplicationDbContext context, IFipeService fipeService)
        {
            _context = context;
            _fipeService = fipeService;
        }

        // LISTAGEM (Index)
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 5;
            ViewData["CurrentFilter"] = searchString;

            IQueryable<Vehicle> vehiclesQuery = GetVehicleQuery(searchString);
            PaginatedList<Vehicle> paginatedResult = await PaginatedList<Vehicle>.CreateAsync(vehiclesQuery, pageNumber ?? 1, pageSize);

            return View(paginatedResult);
        }

        private IQueryable<Vehicle> GetVehicleQuery(string searchString)
        {
            IQueryable<Vehicle> vehiclesQuery = GetBaseQuery();
            vehiclesQuery = ApplySearchFilter(vehiclesQuery, searchString);
            vehiclesQuery = ApplySorting(vehiclesQuery);
            return vehiclesQuery;
        }

        private IQueryable<Vehicle> GetBaseQuery()
        {
            return _context.Vehicles
                .Include(v => v.Manufacturer)
                .AsNoTracking();
        }

        private IQueryable<Vehicle> ApplySearchFilter(IQueryable<Vehicle> query, string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return query;

            return query.Where(s => s.Model.Contains(searchString)
                             || s.Manufacturer.Name.Contains(searchString));
        }

        private IQueryable<Vehicle> ApplySorting(IQueryable<Vehicle> query)
        {
            return query.OrderByDescending(v => v.CreatedAt);
        }

        // CADASTRO (Create)

        public async Task<IActionResult> Create()
        {
            var viewModel = new VehicleFormViewModel
            {
                Manufacturers = await _context.Manufacturers.OrderBy(m => m.Name).ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleFormViewModel viewModel)
        {
            if (ModelState.IsValid)
                return await CreateVehicleAsync(viewModel);

            viewModel.Manufacturers = await _context.Manufacturers.OrderBy(m => m.Name).ToListAsync();
            return View(viewModel);
        }

        private async Task<IActionResult> CreateVehicleAsync(VehicleFormViewModel viewModel)
        {
            var vehicle = new Vehicle
            {
                ManufacturerId = viewModel.ManufacturerId,
                Model = viewModel.Model,
                Year = viewModel.Year,
                Price = viewModel.Price,
                Status = VehicleStatus.Available,
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(vehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // AJAX API (Fipe Integration)
        [HttpGet]
        public async Task<JsonResult> GetModelsByManufacturer(int manufacturerId)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(manufacturerId);

            if (manufacturer == null)
                return Json(new List<object>());

            try
            {
                IEnumerable<FipeReferenceResponse> fipeBrands = await _fipeService.GetBrandsAsync();
                FipeReferenceResponse? selectedFipeBrand = fipeBrands.FirstOrDefault(b => b.Name.Equals(manufacturer.Name, StringComparison.OrdinalIgnoreCase));
                if (selectedFipeBrand != null)
                {
                    IEnumerable<FipeReferenceResponse> models = await _fipeService.GetModelsAsync(selectedFipeBrand.Code);
                    return Json(models.OrderBy(m => m.Name));
                }
            }
            catch {}

            return Json(new List<object>());
        }
    }
}
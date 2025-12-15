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

        #region 1. LEITURA (Listagem)

        [ActionName("Index")]
        public async Task<IActionResult> ListVehicles(string searchString, int? pageNumber)
        {
            int pageSize = 5;
            ViewData["CurrentFilter"] = searchString;

            IQueryable<Vehicle> query = BuildSearchQuery(searchString);
            PaginatedList<Vehicle> paginatedResult = await PaginatedList<Vehicle>.CreateAsync(query, pageNumber ?? 1, pageSize);

            return View(paginatedResult);
        }

        private IQueryable<Vehicle> BuildSearchQuery(string term)
        {
            IQueryable<Vehicle> query = GetBaseQueryWithIncludes();
            query = FilterByBrandOrModel(query, term);
            query = OrderByNewest(query);
            return query;
        }

        private IQueryable<Vehicle> GetBaseQueryWithIncludes()
        {
            return _context.Vehicles.Include(v => v.Manufacturer).AsNoTracking();
        }

        private IQueryable<Vehicle> FilterByBrandOrModel(IQueryable<Vehicle> query, string term)
        {
            if (string.IsNullOrEmpty(term)) return query;
            return query.Where(v => v.Model.Contains(term) || v.Manufacturer.Name.Contains(term));
        }

        private IQueryable<Vehicle> OrderByNewest(IQueryable<Vehicle> query)
        {
            return query.OrderByDescending(v => v.CreatedAt);
        }

        #endregion

        #region 2. ESCRITA (Cadastro)

        [HttpGet]
        [ActionName("Create")]
        public async Task<IActionResult> OpenCreationForm()
        {
            return View("CreationForm", await PrepareEmptyFormAsync());
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCreationForm(VehicleFormViewModel viewModel)
        {
            if (ModelState.IsValid)
                return await PersistVehicleToDatabase(viewModel);

            return View("CreationForm", await ReloadFormWithErrorsAsync(viewModel));
        }

        // --- Helpers de Cadastro ---

        private async Task<VehicleFormViewModel> PrepareEmptyFormAsync()
        {
            return new VehicleFormViewModel
            {
                Manufacturers = await GetManufacturersListAsync()
            };
        }

        private async Task<VehicleFormViewModel> ReloadFormWithErrorsAsync(VehicleFormViewModel viewModel)
        {
            viewModel.Manufacturers = await GetManufacturersListAsync();
            return viewModel;
        }

        private async Task<IEnumerable<Manufacturer>> GetManufacturersListAsync()
        {
            return await _context.Manufacturers.OrderBy(m => m.Name).ToListAsync();
        }

        private async Task<IActionResult> PersistVehicleToDatabase(VehicleFormViewModel viewModel)
        {
            Vehicle vehicle = new()
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
            return RedirectToAction("Index"); ;
        }

        #endregion

        #region  3. API AJAX (FIPE)

        [HttpGet]
        public async Task<JsonResult> GetModelsByManufacturer(int manufacturerId)
        {
            Manufacturer? manufacturer = await _context.Manufacturers.FindAsync(manufacturerId);

            if (manufacturer == null)
                return Json(new List<object>());

            try
            {
                return await FetchModelsFromFipeApi(manufacturer.Name);
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        private async Task<JsonResult> FetchModelsFromFipeApi(string brandName)
        {
            string cleanBrandName = brandName?.Trim() ?? "";

            if (string.IsNullOrEmpty(cleanBrandName))
                return Json(new List<object>());

            IEnumerable<FipeReferenceResponse> fipeBrands = await _fipeService.GetBrandsAsync();

            var selectedFipeBrand = fipeBrands.FirstOrDefault(b =>
            {
                string fipeName = b.Name.Trim();

                bool exactMatch = string.Equals(fipeName, cleanBrandName, StringComparison.CurrentCultureIgnoreCase);

                bool containsMatch = fipeName.IndexOf(cleanBrandName, StringComparison.CurrentCultureIgnoreCase) >= 0;

                bool reverseContains = cleanBrandName.IndexOf(fipeName, StringComparison.CurrentCultureIgnoreCase) >= 0;

                return exactMatch || containsMatch || reverseContains;
            });

            if (selectedFipeBrand != null)
            {
                var models = await _fipeService.GetModelsAsync(selectedFipeBrand.Code.ToString()!);
                return Json(models.OrderBy(m => m.Name));
            }

            return Json(new List<object>());
        }

        #endregion
    }
}
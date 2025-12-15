using AutoNexus.Application.Common;
using AutoNexus.Application.DTOs.Fipe;
using AutoNexus.Application.Interfaces;
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

        #region 1. LEITURA (Listagem / Index)

        [HttpGet]
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
            return _context.Vehicles
                .Include(v => v.Manufacturer)
                .Where(v => !v.IsDeleted)
                .AsNoTracking();
        }

        private IQueryable<Vehicle> FilterByBrandOrModel(IQueryable<Vehicle> query, string term)
        {
            if (string.IsNullOrEmpty(term))
                return query;

            return query.Where(v => v.Model.Contains(term) || v.Manufacturer.Name.Contains(term));
        }

        private IQueryable<Vehicle> OrderByNewest(IQueryable<Vehicle> query)
        {
            return query.OrderByDescending(v => v.CreatedAt);
        }

        #endregion

        #region 2. CRIAÇÃO (Create)

        [HttpGet]
        [ActionName("Create")]
        public async Task<IActionResult> OpenCreationForm()
        {
            VehicleFormViewModel viewModel = await PrepareEmptyFormAsync();
            return View("CreationForm", viewModel);
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCreationForm(VehicleFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                await PersistNewVehicleAsync(viewModel);
                return RedirectToAction("Index");
            }

            VehicleFormViewModel reloadedViewModel = await ReloadFormWithErrorsAsync(viewModel);
            return View("CreationForm", reloadedViewModel);
        }

        private async Task PersistNewVehicleAsync(VehicleFormViewModel viewModel)
        {
            Vehicle vehicle = new Vehicle
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
        }

        #endregion

        #region 3. EDIÇÃO (Edit)

        [HttpGet]
        [ActionName("Edit")]
        public async Task<IActionResult> OpenEditionForm(int? id)
        {
            if (id == null) return NotFound();

            Vehicle? vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null) return NotFound();

            VehicleFormViewModel viewModel = await PrepareEditFormAsync(vehicle);
            return View("EditionForm", viewModel);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitEditionForm(int id, VehicleFormViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await UpdateExistingVehicleAsync(id, viewModel);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Index");
            }

            VehicleFormViewModel reloadedViewModel = await ReloadFormWithErrorsAsync(viewModel);
            return View("EditionForm", reloadedViewModel);
        }

        private async Task<VehicleFormViewModel> PrepareEditFormAsync(Vehicle vehicle)
        {
            return new VehicleFormViewModel
            {
                Id = vehicle.Id,
                ManufacturerId = vehicle.ManufacturerId,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Price = vehicle.Price,
                Manufacturers = await GetManufacturersListAsync()
            };
        }

        private async Task UpdateExistingVehicleAsync(int id, VehicleFormViewModel viewModel)
        {
            Vehicle? vehicleToUpdate = await _context.Vehicles.FindAsync(id);

            if (vehicleToUpdate != null)
            {
                vehicleToUpdate.ManufacturerId = viewModel.ManufacturerId;
                vehicleToUpdate.Model = viewModel.Model;
                vehicleToUpdate.Year = viewModel.Year;
                vehicleToUpdate.Price = viewModel.Price;

                _context.Update(vehicleToUpdate);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region 4. DELEÇÃO (Delete)

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> OpenDeleteConfirmation(int? id)
        {
            if (id == null) return NotFound();

            Vehicle? vehicle = await _context.Vehicles
                .Include(v => v.Manufacturer)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vehicle == null) return NotFound();

            return View("Delete", vehicle);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDeleteConfirmation(int id)
        {
            Vehicle? vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region 5. SERVIÇOS AUXILIARES (Helpers & API)

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
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar modelos FIPE: {ex.Message}");
                return Json(new List<object>());
            }
        }

        private async Task<JsonResult> FetchModelsFromFipeApi(string brandName)
        {
            string cleanBrandName = brandName?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(cleanBrandName))
                return Json(new List<object>());

            IEnumerable<FipeReferenceResponse> fipeBrands = await _fipeService.GetBrandsAsync();
            FipeReferenceResponse? selectedFipeBrand = NewMethod(cleanBrandName, fipeBrands);

            if (selectedFipeBrand != null)
            {
                string brandCode = selectedFipeBrand.Code.ToString()!;
                IEnumerable<FipeReferenceResponse> models = await _fipeService.GetModelsAsync(brandCode);
                return Json(models.OrderBy(m => m.Name));
            }

            return Json(new List<object>());
        }

        private static FipeReferenceResponse? NewMethod(string cleanBrandName, IEnumerable<FipeReferenceResponse> fipeBrands)
        {
            return fipeBrands.FirstOrDefault(b =>
            {
                string fipeName = b.Name.Trim();
                bool exactMatch = string.Equals(fipeName, cleanBrandName, StringComparison.CurrentCultureIgnoreCase);
                bool containsMatch = fipeName.IndexOf(cleanBrandName, StringComparison.CurrentCultureIgnoreCase) >= 0;
                bool reverseContains = cleanBrandName.IndexOf(fipeName, StringComparison.CurrentCultureIgnoreCase) >= 0;

                return exactMatch || containsMatch || reverseContains;
            });
        }

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

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }

        #endregion
    }
}
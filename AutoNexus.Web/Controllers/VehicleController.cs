using AutoNexus.Application.Common;
using AutoNexus.Application.DTOs.Fipe;
using AutoNexus.Application.Interfaces;
using AutoNexus.Domain;
using AutoNexus.Domain.Entities;
using AutoNexus.Domain.Enums;
using AutoNexus.Infrastructure.Data;
using AutoNexus.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AutoNexus.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
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
            ViewData["CurrentFilter"] = searchString;

            IQueryable<Vehicle> query = BuildSearchQuery(searchString);

            PaginatedList<Vehicle> paginatedResult = await PaginatedList<Vehicle>.CreateAsync(query, pageNumber ?? 1, Constants.DEFAULT_PAGE_SIZE);

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

        #region 2. CRIAÇÃO (Create) - PROTEGIDO (ADMIN)

        [HttpGet("create")]
        [ActionName("Create")]
        [Authorize(Roles = Constants.ADMIN_ROLE)]
        public async Task<IActionResult> OpenCreationForm()
        {
            VehicleFormViewModel viewModel = await PrepareEmptyFormAsync();
            return View("CreationForm", viewModel);
        }

        [HttpPost("create")]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.ADMIN_ROLE)]
        public async Task<IActionResult> SubmitCreationForm(VehicleFormViewModel viewModel)
        {
            ValidateYear(viewModel);
            if (ModelState.IsValid)
            {
                await PersistNewVehicleAsync(viewModel);
                return RedirectToAction("Index");
            }

            VehicleFormViewModel reloadedViewModel = await ReloadFormWithErrorsAsync(viewModel);
            return View("CreationForm", reloadedViewModel);
        }

        private void ValidateYear(VehicleFormViewModel viewModel)
        {
            int maxYear = DateTime.Now.Year + 1;
            if (viewModel.Year < 1900 || viewModel.Year > maxYear)
                ModelState.AddModelError("Year", $"O ano deve ser entre 1900 e {maxYear}.");
        }

        private async Task PersistNewVehicleAsync(VehicleFormViewModel viewModel)
        {
            Vehicle vehicle = new Vehicle
            {
                ManufacturerId = viewModel.ManufacturerId,
                Model = viewModel.Model,
                Year = viewModel.Year,
                Price = viewModel.Price,
                Type = viewModel.Type,
                Description = viewModel.Description,
                Status = VehicleStatus.Available,
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(vehicle);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region 3. EDIÇÃO (Edit) - PROTEGIDO (ADMIN)

        [HttpGet("edit/{id}")]
        [ActionName("Edit")]
        [Authorize(Roles = Constants.ADMIN_ROLE)]
        public async Task<IActionResult> OpenEditionForm(int? id)
        {
            if (id == null) return NotFound();

            Vehicle? vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null) return NotFound();

            VehicleFormViewModel viewModel = await PrepareEditFormAsync(vehicle);
            return View("EditionForm", viewModel);
        }

        [HttpPost("edit/{id}")]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.ADMIN_ROLE)]
        public async Task<IActionResult> SubmitEditionForm(int id, VehicleFormViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();
            ValidateYear(viewModel);
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
                Type = vehicle.Type,
                Description = vehicle.Description,
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
                vehicleToUpdate.Type = viewModel.Type;
                vehicleToUpdate.Description = viewModel.Description;

                _context.Update(vehicleToUpdate);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region 4. DELEÇÃO (Delete) - PROTEGIDO (ADMIN)

        [HttpGet("delete/{id}")]
        [ActionName("Delete")]
        [Authorize(Roles = Constants.ADMIN_ROLE)]
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

        [HttpPost("delete/{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.ADMIN_ROLE)]
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

        #region 5. SERVIÇOS AUXILIARES (API / AJAX)

        [HttpGet("models-list/{manufacturerId}")]
        public async Task<JsonResult> GetModelsByManufacturer(int manufacturerId)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(manufacturerId);
            if (manufacturer == null) return Json(new List<object>());

            try
            {
                var fipeBrands = await _fipeService.GetBrandsAsync();

                var matchingBrand = fipeBrands.FirstOrDefault(b =>
                    b.Name.Equals(manufacturer.Name, StringComparison.OrdinalIgnoreCase) ||
                    b.Name.Contains(manufacturer.Name, StringComparison.OrdinalIgnoreCase) ||
                    manufacturer.Name.Contains(b.Name, StringComparison.OrdinalIgnoreCase));

                if (matchingBrand == null) return Json(new List<object>());

                var models = await _fipeService.GetModelsAsync(matchingBrand.Code);

                return Json(models.OrderBy(m => m.Name));
            }
            catch (Exception ex)
            {
                return Json(new List<object> { new { name = $"ERRO: {ex.Message}", code = "" } });
            }
        }

        [HttpGet("years-list/{manufacturerId}/{modelId}")]
        public async Task<JsonResult> GetYearsByModel(int manufacturerId, string modelId)
        {
            try
            {
                string? fipeBrandCode = await GetFipeBrandCodeAsync(manufacturerId);

                if (string.IsNullOrEmpty(fipeBrandCode))
                    return Json(new List<object>());

                IEnumerable<FipeReferenceResponse> years = await _fipeService.GetYearsAsync(fipeBrandCode, modelId);
                return Json(years.OrderBy(y => y.Name));
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        [HttpGet("fipe-details-lookup/{manufacturerId}/{modelId}/{yearId}")]
        public async Task<JsonResult> GetFipeDetails(int manufacturerId, string modelId, string yearId)
        {
            try
            {
                string? fipeBrandCode = await GetFipeBrandCodeAsync(manufacturerId);

                if (string.IsNullOrEmpty(fipeBrandCode))
                    return Json(null);

                FipeVehicleResponse? details = await _fipeService.GetVehicleDetailsAsync(fipeBrandCode, modelId, yearId);

                if (details == null) return Json(null);

                decimal priceValue = ParseFipePrice(details.Price);

                VehicleType typeEnum = MapFipeTypeToDomain(details.VehicleType);

                var result = new
                {
                    price = priceValue,
                    year = details.ModelYear,
                    type = (int)typeEnum,
                    description = $"Combustível: {details.Fuel} | Código Fipe: {details.FipeCode} | Referência: {details.ReferenceMonth}"
                };

                return Json(result);
            }
            catch
            {
                return Json(null);
            }
        }

        #endregion

        #region Helpers Privados

        private async Task<string?> GetFipeBrandCodeAsync(int manufacturerId)
        {
            Manufacturer? manufacturer = await _context.Manufacturers.FindAsync(manufacturerId);
            if (manufacturer == null) return null;

            IEnumerable<FipeReferenceResponse> brands = await _fipeService.GetBrandsAsync();

            FipeReferenceResponse? fipeBrand = brands.FirstOrDefault(b =>
                b.Name.Contains(manufacturer.Name, StringComparison.OrdinalIgnoreCase) ||
                manufacturer.Name.Contains(b.Name, StringComparison.OrdinalIgnoreCase));

            return fipeBrand?.Code?.ToString();
        }

        private decimal ParseFipePrice(string priceString)
        {
            if (string.IsNullOrEmpty(priceString)) return 0;

            string cleanPrice = priceString
                .Replace("R$", "")
                .Replace(".", "")
                .Replace(" ", "")
                .Trim();

            if (decimal.TryParse(cleanPrice, out decimal result))
            {
                return result;
            }
            return 0;
        }

        private VehicleType MapFipeTypeToDomain(int fipeTypeId)
        {
            return fipeTypeId switch
            {
                1 => VehicleType.Car,
                2 => VehicleType.Motorcycle,
                3 => VehicleType.Truck,
                _ => VehicleType.Other
            };
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
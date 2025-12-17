using AutoNexus.Application.Interfaces;
using AutoNexus.Web.Models;
using Microsoft.AspNetCore.Mvc;
using AutoNexus.Application.DTOs.Sales;
using AutoNexus.Domain;

namespace AutoNexus.Web.Controllers
{
    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;

        public SaleController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        #region 1. LEITURA (Index)

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            var paginatedSales = await _saleService.SearchSalesAsync(searchString, pageNumber ?? 1, Constants.DEFAULT_PAGE_SIZE);

            return View(paginatedSales);
        }

        #endregion

        #region 2. CRIAÇÃO (Create)

        [HttpGet]
        [ActionName("Create")]
        public async Task<IActionResult> OpenSaleForm()
        {
            SaleFormViewModel viewModel = await PrepareEmptyFormAsync();
            return View("CreationForm", viewModel);
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSaleForm(SaleFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dto = new CreateSaleDto
                    {
                        VehicleId = viewModel.VehicleId,
                        SalePrice = viewModel.SalePrice,
                        SaleDate = viewModel.SaleDate,
                        ClientName = viewModel.ClientName,
                        ClientCPF = viewModel.ClientCPF,
                        ClientPhone = viewModel.ClientPhone
                    };

                    await _saleService.ProcessSaleAsync(dto);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            SaleFormViewModel reloadedViewModel = await ReloadFormWithErrorsAsync(viewModel);
            return View("CreationForm", reloadedViewModel);
        }

        #endregion

        #region 3. API / AJAX (Novos Métodos para o Front-end)

        /// <summary>
        /// Endpoint chamado pelo JavaScript para buscar dados do cliente pelo CPF
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchClient(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return BadRequest("CPF inválido.");

            var client = await _saleService.GetClientByCpfAsync(cpf);

            if (client == null)
            {
                return NotFound();             }

            return Json(new { name = client.Name, phone = client.Phone });
        }

        #endregion

        #region 4. MÉTODOS AUXILIARES (Helpers)

        private async Task<SaleFormViewModel> PrepareEmptyFormAsync()
        {
            return new SaleFormViewModel
            {
                AvailableVehicles = await _saleService.GetAvailableVehiclesAsync(),
                SaleDate = DateTime.Today
            };
        }

        private async Task<SaleFormViewModel> ReloadFormWithErrorsAsync(SaleFormViewModel viewModel)
        {
            viewModel.AvailableVehicles = await _saleService.GetAvailableVehiclesAsync();
            return viewModel;
        }

        #endregion
    }
}
using AutoNexus.Application.Interfaces;
using AutoNexus.Web.Models;
using Microsoft.AspNetCore.Mvc;
using AutoNexus.Application.DTOs.Sales;

namespace AutoNexus.Web.Controllers
{
    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;

        public SaleController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        #region 1. CRIAÇÃO (Create Sale)

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

                    return RedirectToAction("Index", "Vehicle");
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

        #region 2. MÉTODOS AUXILIARES (Helpers)

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
            // Apenas recarrega a lista do banco, mantendo os dados de input do usuário
            viewModel.AvailableVehicles = await _saleService.GetAvailableVehiclesAsync();
            return viewModel;
        }

        #endregion
    }
}
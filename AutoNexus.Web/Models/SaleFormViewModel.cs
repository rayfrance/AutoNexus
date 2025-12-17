using AutoNexus.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AutoNexus.Web.Models
{
    public class SaleFormViewModel
    {
        [Required(ErrorMessage = "Selecione um veículo.")]
        [Display(Name = "Veículo")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "O preço de venda é obrigatório.")]
        [Display(Name = "Preço Final (R$)")]
        public decimal SalePrice { get; set; }

        [Required(ErrorMessage = "A data da venda é obrigatória.")]
        [Display(Name = "Data da Venda")]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [Display(Name = "Nome do Cliente")]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [Display(Name = "CPF")]
        public string ClientCPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Display(Name = "Telefone")]
        public string ClientPhone { get; set; } = string.Empty;

        public IEnumerable<Vehicle>? AvailableVehicles { get; set; }
    }
}
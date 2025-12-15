using System.ComponentModel.DataAnnotations;
using AutoNexus.Domain.Entities;
using AutoNexus.Domain.Enums;

namespace AutoNexus.Web.Models
{
    public class VehicleFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A Marca é obrigatória")]
        [Display(Name = "Fabricante")]
        public int ManufacturerId { get; set; }

        [Required(ErrorMessage = "O Modelo é obrigatório")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Ano é obrigatório")]
        [Range(1900, 2100, ErrorMessage = "Ano inválido")]
        public int Year { get; set; }

        [Required(ErrorMessage = "O Preço é obrigatório")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public VehicleStatus Status { get; set; }

        // Lista para o Dropdown
        public IEnumerable<Manufacturer>? Manufacturers { get; set; }
    }
}
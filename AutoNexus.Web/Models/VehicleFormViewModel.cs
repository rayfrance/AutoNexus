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

        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [StringLength(100, ErrorMessage = "Máximo de 100 caracteres.")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Ano é obrigatório")]
        [Range(1900, 2100, ErrorMessage = "Ano inválido")]
        public int Year { get; set; }

        [StringLength(500, ErrorMessage = "Máximo de 500 caracteres.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "O Preço é obrigatório")]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser um valor positivo.")] // <--- ADICIONE ISSO
        public decimal Price { get; set; }

        [Required(ErrorMessage = "O tipo é obrigatório.")]
        public VehicleType Type { get; set; }

        public VehicleStatus Status { get; set; }

        public IEnumerable<Manufacturer> Manufacturers { get; set; } = new List<Manufacturer>();
    }
}
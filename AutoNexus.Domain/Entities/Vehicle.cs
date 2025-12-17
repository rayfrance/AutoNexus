using AutoNexus.Domain.Common;
using AutoNexus.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNexus.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public int ManufacturerId { get; set; }

        [ForeignKey("ManufacturerId")]
        public virtual Manufacturer Manufacturer { get; set; } = null!;

        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [StringLength(100, ErrorMessage = "O modelo deve ter no máximo 100 caracteres.")] 
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ano é obrigatório.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser um valor positivo.")] 
        public decimal Price { get; set; }

        [Required(ErrorMessage = "O tipo do veículo é obrigatório.")]
        public VehicleType Type { get; set; }

        [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")] 
        public string? Description { get; set; }
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    }
}
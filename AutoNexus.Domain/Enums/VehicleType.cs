using System.ComponentModel.DataAnnotations;

namespace AutoNexus.Domain.Enums
{
    public enum VehicleType
    {
        [Display(Name = "Carro")]
        Car = 0,

        [Display(Name = "Moto")]
        Motorcycle = 1,

        [Display(Name = "Caminhão")]
        Truck = 2,

        [Display(Name = "Outros")]
        Other = 3
    }
}
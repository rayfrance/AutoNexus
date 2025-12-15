using AutoNexus.Domain.Common;
using AutoNexus.Domain.Enums;

namespace AutoNexus.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public int ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;
    }
}
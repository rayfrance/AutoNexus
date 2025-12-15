using AutoNexus.Domain.Common;

namespace AutoNexus.Domain.Entities
{
    public class Manufacturer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
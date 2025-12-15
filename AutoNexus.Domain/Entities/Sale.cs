using AutoNexus.Domain.Common;

namespace AutoNexus.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        public string ProtocolNumber { get; set; } = string.Empty; // Ex: 202310-ABCD
        public decimal FinalPrice { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
    }
}
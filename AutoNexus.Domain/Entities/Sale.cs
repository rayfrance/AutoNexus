using AutoNexus.Domain.Common;

namespace AutoNexus.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; } = null!;

        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;

        public DateTime SaleDate { get; set; }
        public decimal SalePrice { get; set; }
        public string ProtocolNumber { get; set; } = string.Empty;
    }
}
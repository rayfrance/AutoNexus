namespace AutoNexus.Application.DTOs.Sales
{
    public class CreateSaleDto
    {
        public int VehicleId { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime SaleDate { get; set; }


        public string ClientName { get; set; } = string.Empty;
        public string ClientCPF { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
    }
}
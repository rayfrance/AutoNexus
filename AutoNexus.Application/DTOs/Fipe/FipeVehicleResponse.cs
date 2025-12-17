using System.Text.Json.Serialization;

namespace AutoNexus.Application.DTOs.Fipe
{
    public class FipeVehicleResponse
    {
        [JsonPropertyName("Valor")]
        public string Price { get; set; } = string.Empty;

        [JsonPropertyName("Marca")]
        public string Brand { get; set; } = string.Empty;

        [JsonPropertyName("Modelo")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("AnoModelo")]
        public int ModelYear { get; set; }

        [JsonPropertyName("Combustivel")]
        public string Fuel { get; set; } = string.Empty;

        [JsonPropertyName("CodigoFipe")]
        public string FipeCode { get; set; } = string.Empty;

        [JsonPropertyName("MesReferencia")]
        public string ReferenceMonth { get; set; } = string.Empty;

        [JsonPropertyName("TipoVeiculo")]
        public int VehicleType { get; set; } // 1=Carro, 2=Moto, 3=Caminhão

        [JsonPropertyName("SiglaCombustivel")]
        public string FuelAcronym { get; set; } = string.Empty;
    }
}
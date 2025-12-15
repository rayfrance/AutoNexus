using System.Text.Json.Serialization;

namespace AutoNexus.Application.DTOs.Fipe
{
    public class FipeReferenceResponse
    {
        [JsonPropertyName("nome")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("codigo")]
        public string Code { get; set; } = string.Empty;
    }
}
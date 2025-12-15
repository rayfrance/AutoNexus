using System.Text.Json.Serialization;

namespace AutoNexus.Application.DTOs.Fipe
{
    public class FipeModelResponse
    {
        [JsonPropertyName("modelos")]
        public IEnumerable<FipeReferenceResponse> Modelos { get; set; } = new List<FipeReferenceResponse>();
    }
}
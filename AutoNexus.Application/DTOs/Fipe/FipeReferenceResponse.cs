using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoNexus.Application.DTOs.Fipe
{
    public class FipeReferenceResponse
    {
        [JsonPropertyName("nome")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("codigo")]
        [JsonConverter(typeof(StringOrIntConverter))]
        public string Code { get; set; } = string.Empty;
    }

    public class StringOrIntConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt64().ToString();
            if (reader.TokenType == JsonTokenType.String)
                return reader.GetString()!;

            using (var doc = JsonDocument.ParseValue(ref reader))
                return doc.RootElement.ToString();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
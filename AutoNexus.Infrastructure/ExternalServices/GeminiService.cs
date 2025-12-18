using System.Text;
using System.Text.Json;

public class GeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GeminiService(HttpClient httpClient)
    {
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
        _httpClient = httpClient;
    }

    public async Task<string> GetVendasInsightsAsync(string jsonData, List<string> marcas)
    {
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            string marcasTexto = string.Join(", ", marcas);
            var requestBody = new
            {
                contents = new[] {
                new {
                    parts = new[] {
                        new { text = $"Atue como o Consultor Estratégico da AutoNexus. " +
                                    $"Analise estes dados JSON: {jsonData}. " +
                                    $"INSTRUÇÕES DE CLIMA E FORMATO: " +
                                    $"1. Analise se os dados indicam um bom momento ou alerta. " +
                                    $"2. Se o clima for POSITIVO, comece com '✅ BOA NOTÍCIA:' e use emojis como 🚀, 💰 ou 📈. " +
                                    $"3. Se o clima for NEGATIVO, comece com '⚠️ ATENÇÃO:' e use emojis como 📉, 🚨 ou 🧐. " +
                                    $"4. Sugira uma ação prática focada nestas marcas em estoque: {marcasTexto}. " +
                                    $"5. Máximo 2 frases curtas, sem introduções." }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                return $"Erro na API: {response.StatusCode}. Detalhes: {errorDetails}";
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);

            return doc.RootElement
                      .GetProperty("candidates")[0]
                      .GetProperty("content")
                      .GetProperty("parts")[0]
                      .GetProperty("text")
                      .GetString() ?? "IA não retornou texto.";
        }
        catch (Exception ex)
        {
            return "Erro de conexão: " + ex.Message;
        }
    }
}
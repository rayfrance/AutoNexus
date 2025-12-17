using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

public class GeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GeminiService(IConfiguration config, HttpClient httpClient)
    {
        _apiKey = config["GeminiSettings:ApiKey"];
        _httpClient = httpClient;
    }

    public async Task<string> GetVendasInsightsAsync(string jsonData)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[] {
                new {
                    parts = new[] {
                        new { text = $"Atue como um consultor de vendas de uma revenda de carros chamada AutoNexus. Analise estes dados reais do nosso estoque e vendas em JSON e dê um conselho estratégico curto (máximo 4 linhas) para o gestor: {jsonData}" }
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode) return "Erro ao consultar o consultor IA.";

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        string? geminiTextResult = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
        
        return geminiTextResult;
    }
}
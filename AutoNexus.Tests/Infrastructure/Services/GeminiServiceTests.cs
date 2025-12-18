using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace AutoNexus.Tests.Services
{
    public class GeminiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;

        public GeminiServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);
            Environment.SetEnvironmentVariable("GEMINI_API_KEY", "test_key_nexus");
        }

        [Fact(DisplayName = "GIVEN valid data WHEN " +
            "API returns success " +
            "SHOULD return the generated insight text")]
        public async Task GetVendasInsights_Success_ReturnsText()
        {
            // Arrange
            var expectedText = "✅ BOA NOTÍCIA: Vendas em alta!";
            var apiResponse = new
            {
                candidates = new[] {
                    new { content = new { parts = new[] { new { text = expectedText } } } }
                }
            };

            SetupMockResponse(HttpStatusCode.OK, JsonSerializer.Serialize(apiResponse));
            var service = new GeminiService(_httpClient);

            // Act
            var result = await service.GetVendasInsightsAsync("{}", new List<string> { "BMW" });

            // Assert
            Assert.Equal(expectedText, result);
        }

        [Fact(DisplayName = "GIVEN leaked or invalid key " +
            "WHEN API returns 403 " +
            "SHOULD return formatted Forbidden error")]
        public async Task GetVendasInsights_ApiError_ReturnsForbiddenDetails()
        {
            // Arrange
            var errorDetails = "{\"error\": {\"message\": \"Your API key was reported as leaked.\"}}";
            SetupMockResponse(HttpStatusCode.Forbidden, errorDetails);
            var service = new GeminiService(_httpClient);

            // Act
            var result = await service.GetVendasInsightsAsync("{}", new List<string>());

            // Assert
            Assert.Contains("Erro na API: Forbidden", result);
            Assert.Contains("leaked", result);
        }

        [Fact(DisplayName = "GIVEN a connection failure " +
            "WHEN calling service " +
            "SHOULD catch exception and return connection error")]
        public async Task GetVendasInsights_Exception_ReturnsConnectionErrorMessage()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network failure"));

            var service = new GeminiService(_httpClient);

            // Act
            var result = await service.GetVendasInsightsAsync("{}", new List<string>());

            // Assert
            Assert.Contains("Erro de conexão", result);
            Assert.Contains("Network failure", result);
        }

        private void SetupMockResponse(HttpStatusCode code, string content)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = code,
                    Content = new StringContent(content)
                });
        }
    }
}
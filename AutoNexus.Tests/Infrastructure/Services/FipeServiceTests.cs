using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using AutoNexus.Infrastructure.ExternalServices;
using AutoNexus.Application.DTOs.Fipe;

namespace AutoNexus.Tests.Services
{
    public class FipeServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;

        public FipeServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://parallelum.com.br/fipe/api/v1/")
            };
        }

        [Fact(DisplayName = "GIVEN a valid request " +
            "WHEN fetching brands " +
            "SHOULD return a list of brands")]
        public async Task GetBrands_ValidRequest_ReturnsBrands()
        {
            // Arrange
            var brandsResponse = new List<FipeReferenceResponse>
            {
                new() { Code = "1", Name = "Acura" },
                new() { Code = "2", Name = "Agrale" }
            };

            SetupMockResponse(HttpStatusCode.OK, JsonSerializer.Serialize(brandsResponse));
            var service = new FipeService(_httpClient);

            // Act
            var result = await service.GetBrandsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, b => b.Name == "Acura");
        }

        [Fact(DisplayName = "GIVEN a brand ID " +
            "WHEN fetching models " +
            "SHOULD return models for that brand")]
        public async Task GetModels_ValidBrand_ReturnsModels()
        {
            // Arrange
            var modelsResponse = new FipeModelResponse
            {
                Models = new List<FipeReferenceResponse>
                {
                    new() { Code = "1", Name = "Integra" }
                }
            };

            SetupMockResponse(HttpStatusCode.OK, JsonSerializer.Serialize(modelsResponse));
            var service = new FipeService(_httpClient);

            // Act
            var result = await service.GetModelsAsync("1");

            // Assert
            Assert.Single(result);
            Assert.Equal("Integra", result.First().Name);
        }

        [Fact(DisplayName = "GIVEN invalid parameters " +
            "WHEN fetching vehicle details " +
            "SHOULD return null")]
        public async Task GetVehicleDetails_InvalidParams_ReturnsNull()
        {
            // Arrange
            SetupMockResponse(HttpStatusCode.NotFound, "");
            var service = new FipeService(_httpClient);

            // Act
            var result = await service.GetVehicleDetailsAsync("999", "999", "999");

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "GIVEN brand and model IDs " +
            "WHEN fetching years " +
            "SHOULD return years list from API")]
        public async Task GetYears_ValidRequest_ReturnsYearsList()
        {
            // Arrange
            var yearsResponse = new List<FipeReferenceResponse> { new() { Code = "2024-1", Name = "2024 Gasolina" } };
            SetupMockResponse(HttpStatusCode.OK, JsonSerializer.Serialize(yearsResponse));
            var service = new FipeService(_httpClient);

            // Act
            var result = await service.GetYearsAsync("1", "1");

            // Assert
            Assert.Single(result);
            Assert.Equal("2024-1", result.First().Code);
        }

        [Fact(DisplayName = "GIVEN an API failure " +
            "WHEN fetching brands " +
            "SHOULD throw HttpRequestException")]
        public async Task GetBrands_ApiError_ThrowsHttpRequestException()
        {
            // Arrange
            SetupMockResponse(HttpStatusCode.InternalServerError, "Erro no servidor Fipe");
            var service = new FipeService(_httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetBrandsAsync());
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
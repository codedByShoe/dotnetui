using Moq;
using FluentAssertions;
using Nugetui.Services;
using Moq.Protected;

public class NugetServiceTests
{
  private readonly Mock<HttpMessageHandler> _MockHttpMessageHandler;
  private readonly NugetService _service;

  public NugetServiceTests()
  {
    _MockHttpMessageHandler = new Mock<HttpMessageHandler>();
    var client = new HttpClient(_MockHttpMessageHandler.Object);
    _service = new NugetService(client);
  }

  [Fact]
  public async Task SearchPackagesAsync_WithValidTerm_ReturnsPackages()
  {
    var searchResponse = @"{
            ""totalHits"": 1,
            ""data"": [
                {
                    ""id"": ""Microsoft.EntityFrameworkCore.Sqlite"",
                    ""version"": ""9.0.0"",
                    ""description"": ""Sqlite database provider for Entity Framework Core"",
                    ""totalDownloads"": 166354921
                }
            ]
        }";

    _MockHttpMessageHandler.Protected()
      .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>()
          )
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = System.Net.HttpStatusCode.OK,
        Content = new StringContent(searchResponse)
      });

    var result = await _service.SearchPackagesAsync("Sqlite");

    result.Should().NotBeEmpty();
    result.Should().HaveCount(1);
    result[0].Id.Should().Be("Microsoft.EntityFrameworkCore.Sqlite");
  }

  [Fact]
  public async Task SearchPackagesAsync_WithNetworkError_ReturnsEmptyList()
  {
    _MockHttpMessageHandler.Protected()
      .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>()
          )
      .ThrowsAsync(new HttpRequestException("Network Error"));

    var result = await _service.SearchPackagesAsync("Sqlite");

    result.Should().BeEmpty();
  }
}

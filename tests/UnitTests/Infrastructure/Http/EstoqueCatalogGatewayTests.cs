using System.Net;
using System.Text;
using Infrastructure.Http;

namespace UnitTests.Infrastructure.Http;

public class EstoqueCatalogGatewayTests
{
    [Fact]
    public async Task GetItemByIdAsync_ReturnsDto_WhenOk()
    {
        // Arrange
        var gateway = CreateGateway(
            HttpStatusCode.OK,
            """{"id":1,"codigo":"INS-001","nome":"Óleo","unidadeMedida":"Litro"}""");

        // Act
        var result = await gateway.GetItemByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("INS-001", result.Codigo);
        Assert.Equal("Óleo", result.Nome);
        Assert.Equal("Litro", result.UnidadeMedida);
    }

    [Fact]
    public async Task GetItemByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.NotFound, "");

        // Act
        var result = await gateway.GetItemByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemByIdAsync_ReturnsNull_WhenBodyEmpty()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.OK, "null");

        // Act
        var result = await gateway.GetItemByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    private static EstoqueCatalogGateway CreateGateway(HttpStatusCode status, string body)
    {
        var handler = new StubHttpMessageHandler(status, body);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new EstoqueCatalogGateway(client);
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}

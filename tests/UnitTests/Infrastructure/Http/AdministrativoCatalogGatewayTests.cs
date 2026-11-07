using System.Net;
using System.Text;
using Infrastructure.Http;

namespace UnitTests.Infrastructure.Http;

public class AdministrativoCatalogGatewayTests
{
    [Fact]
    public async Task GetClienteByIdAsync_ReturnsDto_WhenOk()
    {
        // Arrange
        var gateway = CreateGateway(
            HttpStatusCode.OK,
            """{"id":1,"nome":"João","email":"j@t.com","tipoDocumento":"Cpf","documento":"86421537090"}""");

        // Act
        var result = await gateway.GetClienteByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("João", result.Nome);
        Assert.Equal("86421537090", result.Documento);
    }

    [Fact]
    public async Task GetClienteByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.NotFound, "");

        // Act
        var result = await gateway.GetClienteByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetVeiculoByIdAsync_ReturnsDto_WhenOk()
    {
        // Arrange
        var gateway = CreateGateway(
            HttpStatusCode.OK,
            """{"id":2,"idCliente":1,"placa":"ABC1234","marca":"VW","modelo":"Gol"}""");

        // Act
        var result = await gateway.GetVeiculoByIdAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal(1, result.IdCliente);
        Assert.Equal("ABC1234", result.Placa);
    }

    [Fact]
    public async Task GetServicoByIdAsync_ReturnsDto_WhenOk()
    {
        // Arrange
        var gateway = CreateGateway(
            HttpStatusCode.OK,
            """{"id":3,"codigo":"MTR-001","nome":"Troca","precoPadrao":150.5}""");

        // Act
        var result = await gateway.GetServicoByIdAsync(3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("MTR-001", result.Codigo);
        Assert.Equal(150.5m, result.PrecoPadrao);
    }

    [Fact]
    public async Task GetServicoByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.NotFound, "");

        // Act
        var result = await gateway.GetServicoByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetVeiculoByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.NotFound, "");

        // Act
        var result = await gateway.GetVeiculoByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetClienteByIdAsync_ReturnsNull_WhenBodyEmpty()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.OK, "null");

        // Act
        var result = await gateway.GetClienteByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetVeiculoByIdAsync_ReturnsNull_WhenBodyEmpty()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.OK, "null");

        // Act
        var result = await gateway.GetVeiculoByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetServicoByIdAsync_ReturnsNull_WhenBodyEmpty()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.OK, "null");

        // Act
        var result = await gateway.GetServicoByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    private static AdministrativoCatalogGateway CreateGateway(HttpStatusCode status, string body)
    {
        var handler = new StubHttpMessageHandler(status, body);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new AdministrativoCatalogGateway(client);
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

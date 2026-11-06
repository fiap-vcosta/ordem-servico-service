using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.Gateways;

namespace Infrastructure.Http;

public sealed class AdministrativoCatalogGateway(HttpClient httpClient) : IAdministrativoCatalogGateway
{
    public async Task<AdministrativoClienteDto?> GetClienteByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"system/administrativo/clientes/{id}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ClientePayload>(cancellationToken);
        if (body is null)
        {
            return null;
        }

        return new AdministrativoClienteDto(
            body.Id,
            body.Nome,
            body.Email,
            body.TipoDocumento,
            body.Documento);
    }

    public async Task<AdministrativoVeiculoDto?> GetVeiculoByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"system/administrativo/veiculos/{id}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<VeiculoPayload>(cancellationToken);
        if (body is null)
        {
            return null;
        }

        return new AdministrativoVeiculoDto(
            body.Id,
            body.IdCliente,
            body.Placa,
            body.Marca,
            body.Modelo);
    }

    public async Task<AdministrativoServicoDto?> GetServicoByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"system/administrativo/servicos/{id}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ServicoPayload>(cancellationToken);
        if (body is null)
        {
            return null;
        }

        return new AdministrativoServicoDto(
            body.Id,
            body.Codigo,
            body.Nome,
            body.PrecoPadrao);
    }

    private sealed record ClientePayload(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("nome")] string Nome,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("tipoDocumento")] string TipoDocumento,
        [property: JsonPropertyName("documento")] string Documento);

    private sealed record VeiculoPayload(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("idCliente")] int IdCliente,
        [property: JsonPropertyName("placa")] string Placa,
        [property: JsonPropertyName("marca")] string Marca,
        [property: JsonPropertyName("modelo")] string Modelo);

    private sealed record ServicoPayload(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("codigo")] string Codigo,
        [property: JsonPropertyName("nome")] string Nome,
        [property: JsonPropertyName("precoPadrao")] decimal PrecoPadrao);
}

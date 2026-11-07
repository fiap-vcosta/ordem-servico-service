using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.Gateways;

namespace Infrastructure.Http;

public sealed class EstoqueCatalogGateway(HttpClient httpClient) : IEstoqueCatalogGateway
{
    public async Task<EstoqueItemDto?> GetItemByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"system/estoque/itens/{id}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ItemPayload>(cancellationToken);
        if (body is null)
        {
            return null;
        }

        return new EstoqueItemDto(body.Id, body.Codigo, body.Nome, body.UnidadeMedida);
    }

    private sealed record ItemPayload(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("codigo")] string Codigo,
        [property: JsonPropertyName("nome")] string Nome,
        [property: JsonPropertyName("unidadeMedida")] string UnidadeMedida);
}

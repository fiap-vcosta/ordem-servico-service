namespace Application.Abstractions.Gateways;

public interface IEstoqueCatalogGateway
{
    Task<EstoqueItemDto?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default);
}

public sealed record EstoqueItemDto(
    int Id,
    string Codigo,
    string Nome,
    string UnidadeMedida);

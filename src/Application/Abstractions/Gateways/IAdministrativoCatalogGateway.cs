namespace Application.Abstractions.Gateways;

public interface IAdministrativoCatalogGateway
{
    Task<AdministrativoClienteDto?> GetClienteByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<AdministrativoVeiculoDto?> GetVeiculoByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<AdministrativoServicoDto?> GetServicoByIdAsync(int id, CancellationToken cancellationToken = default);
}

public sealed record AdministrativoClienteDto(
    int Id,
    string Nome,
    string Email,
    string TipoDocumento,
    string Documento);

public sealed record AdministrativoVeiculoDto(
    int Id,
    int IdCliente,
    string Placa,
    string Marca,
    string Modelo);

public sealed record AdministrativoServicoDto(
    int Id,
    string Codigo,
    string Nome,
    decimal PrecoPadrao);

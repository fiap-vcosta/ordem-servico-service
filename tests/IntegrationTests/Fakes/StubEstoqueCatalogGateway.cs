using Application.Abstractions.Gateways;

namespace IntegrationTests.Fakes;

public sealed class StubEstoqueCatalogGateway : IEstoqueCatalogGateway
{
    private static readonly Dictionary<int, EstoqueItemDto> Itens = new()
    {
        [1] = new(1, "INS-001", "Óleo Motor Sintético 5W30", "Litro"),
        [2] = new(2, "INS-002", "Óleo Motor Semissintético 15W40", "Litro"),
        [3] = new(3, "PEC-001", "Filtro de Óleo", "Unidade"),
        [4] = new(4, "PEC-002", "Filtro de Ar do Motor", "Unidade"),
        [5] = new(5, "PEC-003", "Filtro de Cabine (Ar Condicionado)", "Unidade"),
        [6] = new(6, "PEC-004", "Pastilha de Freio Dianteira", "Jogo"),
        [7] = new(7, "PEC-005", "Pastilha de Freio Traseira", "Jogo"),
        [8] = new(8, "INS-003", "Fluido de Freio DOT 4", "Frasco"),
        [9] = new(9, "INS-004", "Chumbo para Balanceamento", "Kg"),
        [10] = new(10, "INS-005", "Descarbonizante Spray", "Frasco"),
        [11] = new(11, "PEC-006", "Kit O-ring Bico Injetor", "Jogo"),
        [12] = new(12, "PEC-016", "Correia Dentada", "Unidade"),
        [13] = new(13, "PEC-017", "Tensor da Correia Dentada", "Unidade"),
        [14] = new(14, "PEC-018", "Vela de Ignição", "Jogo"),
        [15] = new(15, "PEC-007", "Bateria 60Ah", "Unidade"),
        [100] = new(100, "TST-100", "Item teste", "Unidade"),
    };

    public Task<EstoqueItemDto?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (Itens.TryGetValue(id, out var item))
        {
            return Task.FromResult<EstoqueItemDto?>(item);
        }

        return Task.FromResult<EstoqueItemDto?>(
            new EstoqueItemDto(id, $"COD-{id}", $"Item {id}", "Unidade"));
    }
}

using Application.Abstractions.Gateways;

namespace IntegrationTests.Fakes;

public sealed class StubAdministrativoCatalogGateway : IAdministrativoCatalogGateway
{
    private static readonly Dictionary<int, AdministrativoClienteDto> Clientes = new()
    {
        [1] = new(1, "João Silva", "joao.silva@email.com", "86421537090"),
        [2] = new(2, "Maria Oliveira", "maria.oliveira@email.com", "71284693031"),
        [3] = new(3, "Carlos Santos", "carlos.santos@email.com", "15350946056"),
        [4] = new(4, "Ana Costa", "ana.costa@email.com", "23100296028"),
        [5] = new(5, "Lucas Ferreira", "lucas.ferreira@email.com", "04531783085"),
        [6] = new(6, "Auto Peças Central Ltda", "contato@autopecascentral.com.br", "11222333000181"),
        [7] = new(7, "Oficina do Toninho ME", "oficinatoninho@email.com", "11444777000161"),
        [8] = new(8, "Transportadora Rápido Sul", "logistica@rapidosul.com.br", "12345678000195"),
        [9] = new(9, "Locadora de Veículos XYZ", "frota@xyzlocadora.com.br", "27865757000102"),
        [10] = new(10, "Comercial de Baterias Potência", "vendas@bateriaspotencia.com.br", "60746948000112"),
    };

    private static readonly Dictionary<int, AdministrativoVeiculoDto> Veiculos = new()
    {
        [1] = new(1, 1, "ABC1234", "Chevrolet", "Onix"),
        [2] = new(2, 1, "XYZ1A23", "Volkswagen", "Gol"),
        [3] = new(3, 1, "DEF5678", "Honda", "Civic"),
        [4] = new(4, 2, "GHI9B12", "Toyota", "Corolla"),
        [5] = new(5, 2, "JKL3456", "Fiat", "Argo"),
        [6] = new(6, 3, "MNO4C56", "Hyundai", "HB20"),
        [7] = new(7, 3, "PQR7890", "Ford", "Ka"),
        [8] = new(8, 3, "STU5D78", "Renault", "Kwid"),
        [9] = new(9, 4, "VWX1234", "Jeep", "Compass"),
        [10] = new(10, 5, "YZA6E90", "Nissan", "Kicks"),
        [11] = new(11, 5, "BCD5678", "Chevrolet", "Tracker"),
        [12] = new(12, 6, "EFG7F12", "Volkswagen", "Saveiro"),
        [13] = new(13, 6, "HIJ9012", "Fiat", "Fiorino"),
        [14] = new(14, 6, "KLM8G34", "Peugeot", "Partner"),
        [15] = new(15, 7, "NOP3456", "Chevrolet", "Montana"),
        [16] = new(16, 8, "QRS9H56", "Mercedes-Benz", "Sprinter"),
        [17] = new(17, 8, "TUV7890", "Iveco", "Daily"),
        [18] = new(18, 8, "WXY0I78", "Ford", "Transit"),
        [19] = new(19, 9, "ZAB1234", "Fiat", "Mobi"),
        [20] = new(20, 10, "CDE1J90", "Volkswagen", "Kombi"),
    };

    private static readonly Dictionary<int, AdministrativoServicoDto> Servicos = new()
    {
        [1] = new(1, "MTR-001", "Troca de Óleo do Motor", 150.00m),
        [2] = new(2, "FLT-002", "Troca de Filtro de Ar", 50.00m),
        [3] = new(3, "FRE-003", "Troca de Pastilhas de Freio", 200.00m),
        [4] = new(4, "SUS-004", "Alinhamento e Balanceamento", 120.00m),
        [5] = new(5, "MTR-005", "Limpeza de Bicos Injetores", 180.00m),
        [6] = new(6, "ELT-006", "Troca de Bateria", 80.00m),
        [7] = new(7, "ARC-007", "Higienização de Ar Condicionado", 130.00m),
        [8] = new(8, "SUS-008", "Troca de Amortecedores Dianteiros", 300.00m),
        [9] = new(9, "EMB-009", "Substituição do Kit de Embreagem", 600.00m),
        [10] = new(10, "GRL-010", "Revisão Preventiva Geral", 250.00m),
    };

    public Task<AdministrativoClienteDto?> GetClienteByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Clientes.TryGetValue(id, out var cliente) ? cliente : null);

    public Task<AdministrativoVeiculoDto?> GetVeiculoByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Veiculos.TryGetValue(id, out var veiculo) ? veiculo : null);

    public Task<AdministrativoServicoDto?> GetServicoByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Servicos.TryGetValue(id, out var servico) ? servico : null);
}

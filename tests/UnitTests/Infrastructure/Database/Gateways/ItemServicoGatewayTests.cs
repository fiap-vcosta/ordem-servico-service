using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure.Database.Gateways;

public class ItemServicoGatewayTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ItemServicoGateway _gateway;
    private readonly OrdemServicoGateway _ordemGateway;

    public ItemServicoGatewayTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _gateway = new ItemServicoGateway(_context);
        _ordemGateway = new OrdemServicoGateway(_context);
    }

    [Fact]
    public async Task GetAllTempoMedioExecucaoAsync_ReturnsAverages_WhenServicesHaveExecutionTimes()
    {
        // Arrange
        var ordem = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "Cliente", Email = "c@t.com" },
            new VeiculoOrdemServico { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" });
        ordem.EnviarParaDiagnostico();
        var catalogo = new ServicoCatalogo { Id = 10, Nome = "Troca", Codigo = "TRC-001" };
        ordem.AdicionarItemServico("Troca", 100m, catalogo, []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();
        ordem.ChecarItensNecessarios(new Dictionary<int, decimal>());
        var servicoId = ordem.Servicos.First().Id;
        ordem.ConfirmarExecucao(
        [
            new ServicoExecutado
            {
                IdServico = servicoId,
                IniciadoEm = DateTime.UtcNow.AddHours(-2),
                FinalizadoEm = DateTime.UtcNow
            }
        ]);
        await _ordemGateway.CriarAsync(ordem);

        // Act
        var tempos = await _gateway.GetAllTempoMedioExecucaoAsync();

        // Assert
        Assert.Contains(tempos, t => t.IdServico == 10 && t.TotalExecucoes == 1 && t.ExecucaoMedia > TimeSpan.Zero);
    }

    [Fact]
    public async Task GetAllTempoMedioExecucaoAsync_ReturnsEmpty_WhenNoCompletedServices()
    {
        // Arrange / Act
        var tempos = await _gateway.GetAllTempoMedioExecucaoAsync();

        // Assert
        Assert.Empty(tempos);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

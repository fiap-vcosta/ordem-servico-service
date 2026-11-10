using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Messaging;
using Moq;

namespace UnitTests.Infrastructure.Messaging;

public class OrdemServicoIntegrationPublisherTests
{
    [Fact]
    public async Task PublishDiagnosticoFinalizadoAsync_PublishesExpectedPayload()
    {
        // Arrange
        var eventPublisher = new Mock<IEventPublisher>();
        var publisher = new OrdemServicoIntegrationPublisher(eventPublisher.Object);
        var ordem = CriarOrdemComServicoAprovado();
        ordem.FinalizarDiagnostico();

        // Act
        await publisher.PublishDiagnosticoFinalizadoAsync(ordem);

        // Assert
        eventPublisher.Verify(
            p => p.PublishAsync(
                TopicIds.DiagnosticoFinalizado,
                EventTypes.DiagnosticoFinalizado,
                It.Is<DiagnosticoFinalizadoData>(d =>
                    d.OrdemServicoId == ordem.Id
                    && d.VersaoDiagnostico == ordem.VersaoDiagnostico
                    && d.Cliente.Nome == ordem.Cliente.Nome
                    && d.Cliente.Email == ordem.Cliente.Email
                    && d.Cliente.Documento == ordem.Cliente.Documento
                    && d.Servicos.Count == 1),
                2,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishOrdemServicoAprovadaAsync_PublishesGroupedItems()
    {
        // Arrange
        var eventPublisher = new Mock<IEventPublisher>();
        var publisher = new OrdemServicoIntegrationPublisher(eventPublisher.Object);
        var ordem = CriarOrdemComServicoAprovado();
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);

        // Act
        await publisher.PublishOrdemServicoAprovadaAsync(ordem);

        // Assert
        eventPublisher.Verify(
            p => p.PublishAsync(
                TopicIds.OrdemServicoAprovada,
                EventTypes.OrdemServicoAprovada,
                It.Is<OrdemServicoAprovadaData>(d =>
                    d.OrdemServicoId == ordem.Id
                    && d.Itens.Count == 1
                    && d.Itens[0].ItemEstoqueId == 100
                    && d.Itens[0].Quantidade == 2m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishOrdemServicoAprovadaAsync_Throws_WhenAprovadaEmIsNull()
    {
        // Arrange
        var eventPublisher = new Mock<IEventPublisher>();
        var publisher = new OrdemServicoIntegrationPublisher(eventPublisher.Object);
        var ordem = CriarOrdemBase();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico(
            "Troca",
            150m,
            new ServicoCatalogo { Id = 1, Codigo = "MTR-001", Nome = "Troca" },
            []);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            publisher.PublishOrdemServicoAprovadaAsync(ordem));
    }

    [Fact]
    public async Task PublishDiagnosticoFinalizadoAsync_MapsSugeridoStatus()
    {
        // Arrange
        var eventPublisher = new Mock<IEventPublisher>();
        var publisher = new OrdemServicoIntegrationPublisher(eventPublisher.Object);
        var ordem = CriarOrdemBase();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico(
            "Troca",
            150m,
            new ServicoCatalogo { Id = 1, Codigo = "MTR-001", Nome = "Troca" },
            []);
        ordem.FinalizarDiagnostico();

        // Act
        await publisher.PublishDiagnosticoFinalizadoAsync(ordem);

        // Assert
        eventPublisher.Verify(
            p => p.PublishAsync(
                TopicIds.DiagnosticoFinalizado,
                EventTypes.DiagnosticoFinalizado,
                It.Is<DiagnosticoFinalizadoData>(d =>
                    d.Servicos.Count == 1
                    && d.Servicos[0].Status == ServicoDiagnosticoStatus.Sugerido),
                2,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishDiagnosticoFinalizadoAsync_MapsRejeitadoStatus()
    {
        // Arrange
        var eventPublisher = new Mock<IEventPublisher>();
        var publisher = new OrdemServicoIntegrationPublisher(eventPublisher.Object);
        var ordem = CriarOrdemBase();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico(
            "Troca",
            150m,
            new ServicoCatalogo { Id = 1, Codigo = "MTR-001", Nome = "Troca" },
            []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(ordem.VersaoDiagnostico);
        ordem.RejeitarServicosSugeridos();

        // Act
        await publisher.PublishDiagnosticoFinalizadoAsync(ordem);

        // Assert
        eventPublisher.Verify(
            p => p.PublishAsync(
                TopicIds.DiagnosticoFinalizado,
                EventTypes.DiagnosticoFinalizado,
                It.Is<DiagnosticoFinalizadoData>(d =>
                    d.Servicos.Count == 1
                    && d.Servicos[0].Status == ServicoDiagnosticoStatus.Rejeitado),
                2,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishDiagnosticoFinalizadoAsync_MapsConcluidoStatus()
    {
        // Arrange
        var eventPublisher = new Mock<IEventPublisher>();
        var publisher = new OrdemServicoIntegrationPublisher(eventPublisher.Object);
        var ordem = CriarOrdemComServicoAprovado();
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        var iniciado = DateTime.UtcNow.AddHours(-1);
        var finalizado = DateTime.UtcNow;
        foreach (var servico in ordem.Servicos)
        {
            servico.ConfirmarConclusao(iniciado, finalizado);
        }

        // Act
        await publisher.PublishDiagnosticoFinalizadoAsync(ordem);

        // Assert
        eventPublisher.Verify(
            p => p.PublishAsync(
                TopicIds.DiagnosticoFinalizado,
                EventTypes.DiagnosticoFinalizado,
                It.Is<DiagnosticoFinalizadoData>(d =>
                    d.Servicos.Count == 1
                    && d.Servicos[0].Status == ServicoDiagnosticoStatus.Concluido),
                2,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishOrdemServicoFinalizadaAsync_PublishesId()
    {
        // Arrange
        var eventPublisher = new Mock<IEventPublisher>();
        var publisher = new OrdemServicoIntegrationPublisher(eventPublisher.Object);
        var ordem = CriarOrdemComServicoAprovado();

        // Act
        await publisher.PublishOrdemServicoFinalizadaAsync(ordem);

        // Assert
        eventPublisher.Verify(
            p => p.PublishAsync(
                TopicIds.OrdemServicoFinalizada,
                EventTypes.OrdemServicoFinalizada,
                It.Is<OrdemServicoFinalizadaData>(d => d.OrdemServicoId == ordem.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishOrdemServicoDescartadaAsync_PublishesId()
    {
        // Arrange
        var eventPublisher = new Mock<IEventPublisher>();
        var publisher = new OrdemServicoIntegrationPublisher(eventPublisher.Object);
        var ordem = CriarOrdemComServicoAprovado();

        // Act
        await publisher.PublishOrdemServicoDescartadaAsync(ordem);

        // Assert
        eventPublisher.Verify(
            p => p.PublishAsync(
                TopicIds.OrdemServicoDescartada,
                EventTypes.OrdemServicoDescartada,
                It.Is<OrdemServicoDescartadaData>(d => d.OrdemServicoId == ordem.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static OrdemServicoAggregateRoot CriarOrdemBase() =>
        OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "Cliente", Email = "c@t.com", Documento = "86421537090" },
            new VeiculoOrdemServico { Placa = "ABC1234", Marca = "VW", Modelo = "Gol" });

    private static OrdemServicoAggregateRoot CriarOrdemComServicoAprovado()
    {
        var ordem = CriarOrdemBase();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico(
            "Troca",
            150m,
            new ServicoCatalogo { Id = 1, Codigo = "MTR-001", Nome = "Troca" },
            [
                new ItemNecessario.CriarItemNecessarioParams(
                    0,
                    2m,
                    new ItemEstoqueOrdemServico { Id = 100, Codigo = "PEC", Nome = "Peça", UnidadeMedida = "Unidade" })
            ]);
        foreach (var servico in ordem.Servicos)
        {
            servico.Aprovar();
        }

        return ordem;
    }
}

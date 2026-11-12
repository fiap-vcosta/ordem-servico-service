using UnitTests.Support;
using Application.Abstractions.Gateways;
using Application.Abstractions.Messaging;
using Application.UseCases.Commands.AplicarOrcamentoGerado;
using Domain.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests.Application.UseCases.Commands.AplicarOrcamentoGerado;

public class AplicarOrcamentoGeradoCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithSuggestedServices_GoesToAguardandoAprovacao()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var publisher = new Mock<IOrdemServicoIntegrationPublisher>();
        var ordem = CriarOrdemAguardandoOrcamentoComSugerido();
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            publisher.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = OrdemServicoTestIds.Of(1), VersaoDiagnostico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, result.Status);
        publisher.Verify(
            p => p.PublishOrdemServicoAprovadaAsync(It.IsAny<OrdemServicoAggregateRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithOnlyApprovedServices_GoesToChecandoEstoqueAndPublishes()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var publisher = new Mock<IOrdemServicoIntegrationPublisher>();
        var ordem = CriarOrdemAguardandoOrcamentoSoComAprovados();
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        publisher
            .Setup(p => p.PublishOrdemServicoAprovadaAsync(It.IsAny<OrdemServicoAggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            publisher.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = OrdemServicoTestIds.Of(1), VersaoDiagnostico = 2 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.ChecandoEstoque, result.Status);
        publisher.Verify(
            p => p.PublishOrdemServicoAprovadaAsync(ordem, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithOnlyRejectedServices_GoesToEntregue()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var publisher = new Mock<IOrdemServicoIntegrationPublisher>();
        var ordem = CriarOrdemAguardandoOrcamentoSoComRejeitados();
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            publisher.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = OrdemServicoTestIds.Of(1), VersaoDiagnostico = 2 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.Entregue, result.Status);
        publisher.Verify(
            p => p.PublishOrdemServicoAprovadaAsync(It.IsAny<OrdemServicoAggregateRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenWrongStatus_IsIdempotentNoOp()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var publisher = new Mock<IOrdemServicoIntegrationPublisher>();
        var ordem = CriarOrdemAguardandoOrcamentoComSugerido();
        ordem.AplicarOrcamentoGerado(1);
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordem);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            publisher.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = OrdemServicoTestIds.Of(1), VersaoDiagnostico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, result.Status);
        gateway.Verify(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenWrongVersion_IsIdempotentNoOp()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var publisher = new Mock<IOrdemServicoIntegrationPublisher>();
        var ordem = CriarOrdemAguardandoOrcamentoComSugerido();
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordem);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            publisher.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = OrdemServicoTestIds.Of(1), VersaoDiagnostico = 99 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoOrcamento, result.Status);
        gateway.Verify(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsDomainNotFoundException()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var publisher = new Mock<IOrdemServicoIntegrationPublisher>();
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(999))).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            publisher.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(
                new AplicarOrcamentoGeradoCommand { IdOrdemServico = OrdemServicoTestIds.Of(999), VersaoDiagnostico = 1 },
                CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemAguardandoOrcamentoComSugerido()
    {
        var ordem = CriarBase();
        ordem.AdicionarItemServico(
            "Troca",
            100m,
            new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" },
            []);
        ordem.FinalizarDiagnostico();
        return ordem;
    }

    private static OrdemServicoAggregateRoot CriarOrdemAguardandoOrcamentoSoComAprovados()
    {
        var ordem = CriarBase();
        ordem.AdicionarItemServico(
            "Troca",
            100m,
            new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" },
            []);
        ordem.AdicionarItemServico(
            "Extra",
            50m,
            new ServicoCatalogo { Id = 2, Nome = "Extra", Codigo = "SVR-002" },
            []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        var primeiroId = ordem.Servicos.First().Id;
        ordem.AprovarServicosParcialmente([primeiroId]);
        ordem.FinalizarDiagnostico();
        return ordem;
    }

    private static OrdemServicoAggregateRoot CriarOrdemAguardandoOrcamentoSoComRejeitados()
    {
        var ordem = CriarBase();
        ordem.AdicionarItemServico(
            "Troca",
            100m,
            new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" },
            []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.RejeitarServicosSugeridos();
        ordem.FinalizarDiagnostico();
        return ordem;
    }

    private static OrdemServicoAggregateRoot CriarBase()
    {
        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordem = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ordem.EnviarParaDiagnostico();
        typeof(OrdemServicoAggregateRoot).GetProperty("Id")!.SetValue(ordem, OrdemServicoTestIds.Of(1));
        return ordem;
    }
}

using Application.Abstractions.Events;
using Application.Abstractions.Gateways;
using Application.UseCases.OrdemServico.Commands.AplicarOrcamentoGerado;
using Domain.Exceptions;
using Domain.OrdemServico.Entities;
using Domain.OrdemServico.Events;
using Domain.OrdemServico.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests.Application.UseCases.OrdemServico.Commands.AplicarOrcamentoGerado;

public class AplicarOrcamentoGeradoCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithSuggestedServices_GoesToAguardandoAprovacao()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var mediator = new Mock<IMediator>();
        var ordem = CriarOrdemAguardandoOrcamentoComSugerido();
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            mediator.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = 1, VersaoDiagnostico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, result.Status);
        mediator.Verify(
            m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoAprovadaEvent>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithOnlyApprovedServices_GoesToChecandoEstoqueAndPublishes()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var mediator = new Mock<IMediator>();
        var ordem = CriarOrdemAguardandoOrcamentoSoComAprovados();
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        mediator
            .Setup(m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoAprovadaEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            mediator.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = 1, VersaoDiagnostico = 2 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.ChecandoEstoque, result.Status);
        mediator.Verify(
            m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoAprovadaEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithOnlyRejectedServices_GoesToEntregue()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var mediator = new Mock<IMediator>();
        var ordem = CriarOrdemAguardandoOrcamentoSoComRejeitados();
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            mediator.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = 1, VersaoDiagnostico = 2 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.Entregue, result.Status);
        mediator.Verify(
            m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoAprovadaEvent>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenWrongStatus_IsIdempotentNoOp()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var mediator = new Mock<IMediator>();
        var ordem = CriarOrdemAguardandoOrcamentoComSugerido();
        ordem.AplicarOrcamentoGerado(1);
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            mediator.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = 1, VersaoDiagnostico = 1 },
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
        var mediator = new Mock<IMediator>();
        var ordem = CriarOrdemAguardandoOrcamentoComSugerido();
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            mediator.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoGeradoCommand { IdOrdemServico = 1, VersaoDiagnostico = 99 },
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
        var mediator = new Mock<IMediator>();
        gateway.Setup(g => g.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new AplicarOrcamentoGeradoCommandHandler(
            gateway.Object,
            mediator.Object,
            NullLogger<AplicarOrcamentoGeradoCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(
                new AplicarOrcamentoGeradoCommand { IdOrdemServico = 999, VersaoDiagnostico = 1 },
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
        typeof(OrdemServicoAggregateRoot).GetProperty("Id")!.SetValue(ordem, 1);
        return ordem;
    }
}

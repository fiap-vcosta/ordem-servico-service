using Application.Abstractions.Events;
using Application.Abstractions.Gateways;
using Application.UseCases.OrdemServico.Commands.AplicarOrcamentoGerado;
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
        var ordem = CriarOrdemAguardandoOrcamento();
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
    public async Task Handle_WhenWrongStatus_IsIdempotentNoOp()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var mediator = new Mock<IMediator>();
        var ordem = CriarOrdemAguardandoOrcamento();
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

    private static OrdemServicoAggregateRoot CriarOrdemAguardandoOrcamento()
    {
        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordem = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico(
            "Troca",
            100m,
            new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" },
            []);
        ordem.FinalizarDiagnostico();
        typeof(OrdemServicoAggregateRoot).GetProperty("Id")!.SetValue(ordem, 1);
        return ordem;
    }
}

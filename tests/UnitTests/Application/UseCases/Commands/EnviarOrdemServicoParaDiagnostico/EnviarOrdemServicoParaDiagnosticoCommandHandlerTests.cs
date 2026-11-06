using Application.Abstractions.Events;
using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Application.UseCases.Commands.EnviarOrdemServicoParaDiagnostico;
using Domain.Entities;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;
using MediatR;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.Commands.EnviarOrdemServicoParaDiagnostico;

public class EnviarOrdemServicoParaDiagnosticoCommandHandlerTests
{
    [Fact]
    public async Task Handle_MovesToDiagnosticoAndNotifies()
    {
        // Arrange
        var ordemServicoGateway = new Mock<IOrdemServicoGateway>();
        var notificacaoService = new Mock<INotificacaoService>();
        var mediator = new Mock<IMediator>();

        var ordem = CriarOrdemBasica();
        ReflectSetId(ordem, 7);

        ordemServicoGateway.Setup(g => g.GetByIdAsync(7)).ReturnsAsync(ordem);
        ordemServicoGateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        notificacaoService
            .Setup(n => n.NotificarUsuariosPorTipo("Mecanico", It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mediator
            .Setup(m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoRecebidaDiagnosticoEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new EnviarOrdemServicoParaDiagnosticoCommandHandler(
            ordemServicoGateway.Object,
            notificacaoService.Object,
            mediator.Object,
            NullLogger<EnviarOrdemServicoParaDiagnosticoCommandHandler>.Instance);

        // Act
        await handler.Handle(new EnviarOrdemServicoParaDiagnosticoCommand { IdOrdemServico = 7 }, CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordem.Status);
        ordemServicoGateway.Verify(g => g.UpdateAsync(ordem), Times.Once);
        notificacaoService.Verify(
            n => n.NotificarUsuariosPorTipo("Mecanico", It.Is<string>(s => s.Contains('7'))),
            Times.Once);
        mediator.Verify(
            m => m.Publish(It.Is<DomainEventNotification<OrdemServicoRecebidaDiagnosticoEvent>>(n => n.DomainEvent.IdOrdemServico == 7), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_WhenOrdemNotFound()
    {
        // Arrange
        var ordemServicoGateway = new Mock<IOrdemServicoGateway>();
        ordemServicoGateway.Setup(g => g.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);
        var handler = new EnviarOrdemServicoParaDiagnosticoCommandHandler(
            ordemServicoGateway.Object,
            new Mock<INotificacaoService>().Object,
            new Mock<IMediator>().Object,
            NullLogger<EnviarOrdemServicoParaDiagnosticoCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(new EnviarOrdemServicoParaDiagnosticoCommand { IdOrdemServico = 999 }, CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemBasica()
    {
        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        return OrdemServicoAggregateRoot.Criar(cliente, veiculo);
    }

    private static void ReflectSetId(OrdemServicoAggregateRoot ordem, int id)
    {
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordem, id);
    }
}

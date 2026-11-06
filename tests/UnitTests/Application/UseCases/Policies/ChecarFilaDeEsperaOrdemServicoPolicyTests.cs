using Application.Abstractions.Events;
using Application.UseCases.Commands.AlocarEstoqueOrdemServico;
using Application.UseCases.Policies;
using Application.Abstractions.Gateways;
using Domain.Estoque.Entities;
using Domain.Estoque.Events;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Moq;

namespace UnitTests.Application.UseCases.Policies;

public class ChecarFilaDeEsperaOrdemServicoPolicyTests
{
    [Fact]
    public async Task Handle_AllocatesOrdersWaitingForPart()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockMediator = new Mock<IMediator>();

        var ordem1 = CriarOrdemBasica();
        ReflectSetId(ordem1, 10);
        var ordem2 = CriarOrdemBasica();
        ReflectSetId(ordem2, 20);

        var item = new ItemEstoqueAggregateRoot { Id = 100, Nome = "Pneu", Saldo = 50m };

        mockOrdemServicoGateway
            .Setup(r => r.GetAguardandoPecaPorItemEstoqueAsync(100))
            .ReturnsAsync(new List<OrdemServicoAggregateRoot> { ordem1, ordem2 });
        mockMediator
            .Setup(m => m.Send(It.IsAny<AlocarEstoqueOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var policy = new ChecarFilaDeEsperaOrdemServicoPolicy(mockOrdemServicoGateway.Object, mockMediator.Object);

        // Act
        await policy.Handle(new DomainEventNotification<ChegadaDeItensRegistradaEvent>(new ChegadaDeItensRegistradaEvent(item)), CancellationToken.None);

        // Assert
        mockMediator.Verify(
            m => m.Send(It.Is<AlocarEstoqueOrdemServicoCommand>(c => c.IdOrdemServico == 10), It.IsAny<CancellationToken>()),
            Times.Once);
        mockMediator.Verify(
            m => m.Send(It.Is<AlocarEstoqueOrdemServicoCommand>(c => c.IdOrdemServico == 20), It.IsAny<CancellationToken>()),
            Times.Once);
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

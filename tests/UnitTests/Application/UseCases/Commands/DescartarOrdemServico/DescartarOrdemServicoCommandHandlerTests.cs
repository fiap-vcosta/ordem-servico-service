using Application.Abstractions.Events;
using Domain.Exceptions;
using Application.UseCases.Commands.DescartarOrdemServico;
using Application.Abstractions.Gateways;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.Commands.DescartarOrdemServico;

public class DescartarOrdemServicoCommandHandlerTests
{
    [Fact]
    public async Task Handle_DiscardsOrdemServico_WhenCommandIsValid()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockMediator = new Mock<IMediator>();

        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordemServico = OrdemServicoAggregateRoot.Criar(cliente, veiculo);

        var command = new DescartarOrdemServicoCommand { IdOrdemServico = 1 };

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordemServico);
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        mockMediator.Setup(m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoDescartadaEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new DescartarOrdemServicoCommandHandler(mockOrdemServicoGateway.Object, mockMediator.Object,
            NullLogger<DescartarOrdemServicoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusOrdemServico.Descartada, result.Status);
        mockOrdemServicoGateway.Verify(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsDomainNotFoundException_WhenOrdemServicoNotFound()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new DescartarOrdemServicoCommandHandler(mockOrdemServicoGateway.Object, new Mock<IMediator>().Object,
            NullLogger<DescartarOrdemServicoCommandHandler>.Instance);
        var command = new DescartarOrdemServicoCommand { IdOrdemServico = 999 };

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}

using Application.Abstractions.Events;
using Application.UseCases.Commands.AlocarEstoqueOrdemServico;
using Application.UseCases.Policies;
using Domain.Events;
using MediatR;
using Moq;

namespace UnitTests.Application.UseCases.Policies;

public class ChecarEstoqueOrdemServicoPolicyTests
{
    [Fact]
    public async Task Handle_SendsAlocarEstoqueCommand()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        mockMediator
            .Setup(m => m.Send(It.IsAny<AlocarEstoqueOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        var policy = new ChecarEstoqueOrdemServicoPolicy(mockMediator.Object);

        // Act
        await policy.Handle(new DomainEventNotification<OrdemServicoAprovadaEvent>(new OrdemServicoAprovadaEvent(42)), CancellationToken.None);

        // Assert
        mockMediator.Verify(
            m => m.Send(It.Is<AlocarEstoqueOrdemServicoCommand>(c => c.IdOrdemServico == 42), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

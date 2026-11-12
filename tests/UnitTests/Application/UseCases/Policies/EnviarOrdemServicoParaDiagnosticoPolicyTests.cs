using UnitTests.Support;
using Application.Abstractions.Events;
using Application.UseCases.Commands.EnviarOrdemServicoParaDiagnostico;
using Application.UseCases.Policies;
using Domain.Events;
using MediatR;
using Moq;

namespace UnitTests.Application.UseCases.Policies;

public class EnviarOrdemServicoParaDiagnosticoPolicyTests
{
    [Fact]
    public async Task Handle_SendsEnviarOrdemServicoParaDiagnosticoCommand()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<EnviarOrdemServicoParaDiagnosticoCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var policy = new EnviarOrdemServicoParaDiagnosticoPolicy(mediator.Object);

        // Act
        await policy.Handle(new DomainEventNotification<OrdemServicoCriadaEvent>(new OrdemServicoCriadaEvent(OrdemServicoTestIds.Of(7))), CancellationToken.None);

        // Assert
        mediator.Verify(
            m => m.Send(It.Is<EnviarOrdemServicoParaDiagnosticoCommand>(c => c.IdOrdemServico == OrdemServicoTestIds.Of(7)), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

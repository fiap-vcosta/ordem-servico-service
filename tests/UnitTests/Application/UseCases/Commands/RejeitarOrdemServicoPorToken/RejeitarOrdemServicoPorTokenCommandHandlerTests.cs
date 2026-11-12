using UnitTests.Support;
using Application.UseCases.Commands.RejeitarOrdemServico;
using Application.UseCases.Commands.RejeitarOrdemServicoPorToken;
using Application.UseCases.Queries.GetOrdemServicoByTokenEDocumento;
using Application.UseCases.Responses;
using Domain.Exceptions;
using Domain.Entities;
using MediatR;
using Moq;

namespace UnitTests.Application.UseCases.Commands.RejeitarOrdemServicoPorToken;

public class RejeitarOrdemServicoPorTokenCommandHandlerTests
{
    private const string DocumentoJoaoSilva = "86421537090";
    private const string DocumentoMariaOliveira = "71284693031";

    [Fact]
    public async Task Handle_SendsRejeitarCommand_WhenTokenAndOwnershipMatch()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(
                It.Is<GetOrdemServicoByTokenEDocumentoQuery>(q =>
                    q.TokenAprovacao == "token-ok" && q.DocumentoCliente == DocumentoJoaoSilva),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrdemServicoTestIds.Of(9));
        mediator
            .Setup(m => m.Send(It.IsAny<RejeitarOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RejeitarOrdemServicoCommandResponse
            {
                Id = OrdemServicoTestIds.Of(9),
                Status = StatusOrdemServico.EmDiagnostico,
                ValorTotal = 0m,
                RecebidaEm = DateTime.UtcNow,
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "João", Email = "joao@teste.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" },
                Servicos = []
            });

        var handler = new RejeitarOrdemServicoPorTokenCommandHandler(mediator.Object);

        // Act
        var result = await handler.Handle(
            new RejeitarOrdemServicoPorTokenCommand
            {
                TokenAprovacao = "token-ok",
                DocumentoCliente = DocumentoJoaoSilva
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(OrdemServicoTestIds.Of(9), result.Id);
        mediator.Verify(
            m => m.Send(It.Is<RejeitarOrdemServicoCommand>(c => c.IdOrdemServico == OrdemServicoTestIds.Of(9)), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_WhenTokenNotFound()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<GetOrdemServicoByTokenEDocumentoQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainNotFoundException("não encontrada"));
        var handler = new RejeitarOrdemServicoPorTokenCommandHandler(mediator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(
                new RejeitarOrdemServicoPorTokenCommand
                {
                    TokenAprovacao = "invalid",
                    DocumentoCliente = DocumentoJoaoSilva
                },
                CancellationToken.None));
        mediator.Verify(
            m => m.Send(It.IsAny<RejeitarOrdemServicoCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Throws_WhenOwnershipMismatch()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(
                It.Is<GetOrdemServicoByTokenEDocumentoQuery>(q => q.DocumentoCliente == DocumentoMariaOliveira),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainNotFoundException("não encontrada"));
        var handler = new RejeitarOrdemServicoPorTokenCommandHandler(mediator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(
                new RejeitarOrdemServicoPorTokenCommand
                {
                    TokenAprovacao = "token-ok",
                    DocumentoCliente = DocumentoMariaOliveira
                },
                CancellationToken.None));
        mediator.Verify(
            m => m.Send(It.IsAny<RejeitarOrdemServicoCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

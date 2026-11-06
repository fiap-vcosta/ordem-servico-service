using Application.UseCases.Commands.AprovarOrdemServico;
using Application.UseCases.Commands.AprovarOrdemServicoPorToken;
using Application.UseCases.Queries.GetOrdemServicoByTokenEDocumento;
using Application.UseCases.Responses;
using Domain.Exceptions;
using Domain.Entities;
using MediatR;
using Moq;

namespace UnitTests.Application.UseCases.Commands.AprovarOrdemServicoPorToken;

public class AprovarOrdemServicoPorTokenCommandHandlerTests
{
    private const string DocumentoJoaoSilva = "86421537090";
    private const string DocumentoMariaOliveira = "71284693031";

    [Fact]
    public async Task Handle_SendsAprovarCommand_WhenTokenAndOwnershipMatch()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(
                It.Is<GetOrdemServicoByTokenEDocumentoQuery>(q =>
                    q.TokenAprovacao == "token-ok" && q.DocumentoCliente == DocumentoJoaoSilva),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);
        mediator
            .Setup(m => m.Send(It.IsAny<AprovarOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AprovarOrdemServicoCommandResponse
            {
                Id = 7,
                Status = StatusOrdemServico.ChecandoEstoque,
                ValorTotal = 100m,
                RecebidaEm = DateTime.UtcNow,
                AprovadaEm = DateTime.UtcNow,
                Cliente = new ClienteOrdemServicoResponse { Id = 1, Nome = "João", Email = "joao@teste.com" },
                Veiculo = new VeiculoOrdemServicoResponse { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" },
                Servicos = []
            });

        var handler = new AprovarOrdemServicoPorTokenCommandHandler(mediator.Object);

        // Act
        var result = await handler.Handle(
            new AprovarOrdemServicoPorTokenCommand
            {
                TokenAprovacao = "token-ok",
                DocumentoCliente = DocumentoJoaoSilva
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(7, result.Id);
        mediator.Verify(
            m => m.Send(It.Is<AprovarOrdemServicoCommand>(c => c.IdOrdemServico == 7), It.IsAny<CancellationToken>()),
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
        var handler = new AprovarOrdemServicoPorTokenCommandHandler(mediator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(
                new AprovarOrdemServicoPorTokenCommand
                {
                    TokenAprovacao = "invalid",
                    DocumentoCliente = DocumentoJoaoSilva
                },
                CancellationToken.None));
        mediator.Verify(
            m => m.Send(It.IsAny<AprovarOrdemServicoCommand>(), It.IsAny<CancellationToken>()),
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
        var handler = new AprovarOrdemServicoPorTokenCommandHandler(mediator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(
                new AprovarOrdemServicoPorTokenCommand
                {
                    TokenAprovacao = "token-ok",
                    DocumentoCliente = DocumentoMariaOliveira
                },
                CancellationToken.None));
        mediator.Verify(
            m => m.Send(It.IsAny<AprovarOrdemServicoCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

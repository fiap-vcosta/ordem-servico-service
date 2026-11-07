using Application.Abstractions.Gateways;
using Application.UseCases.Queries.GetOrdemServicoByTokenEDocumento;
using Domain.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using Moq;

namespace UnitTests.Application.UseCases.Queries.GetOrdemServicoByTokenEDocumento;

public class GetOrdemServicoByTokenEDocumentoQueryHandlerTests
{
    private const string DocumentoJoaoSilva = "86421537090";

    [Fact]
    public async Task Handle_ReturnsOrdemId_WhenTokenAndDocumentoMatch()
    {
        // Arrange
        var ordem = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "João", Email = "joao@teste.com" },
            new VeiculoOrdemServico { Placa = "ABC-1234", Marca = "VW", Modelo = "Gol" });
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordem, 7);

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway
            .Setup(g => g.GetByTokenEDocumentoAsync(ordem.TokenAprovacao, DocumentoJoaoSilva))
            .ReturnsAsync(ordem);

        var handler = new GetOrdemServicoByTokenEDocumentoQueryHandler(gateway.Object);

        // Act
        var id = await handler.Handle(
            new GetOrdemServicoByTokenEDocumentoQuery
            {
                TokenAprovacao = ordem.TokenAprovacao,
                DocumentoCliente = DocumentoJoaoSilva
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(7, id);
    }

    [Fact]
    public async Task Handle_Throws_WhenNotFound()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway
            .Setup(g => g.GetByTokenEDocumentoAsync("invalid", DocumentoJoaoSilva))
            .ReturnsAsync((OrdemServicoAggregateRoot?)null);
        var handler = new GetOrdemServicoByTokenEDocumentoQueryHandler(gateway.Object);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(
                new GetOrdemServicoByTokenEDocumentoQuery
                {
                    TokenAprovacao = "invalid",
                    DocumentoCliente = DocumentoJoaoSilva
                },
                CancellationToken.None));
    }
}

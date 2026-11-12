using UnitTests.Support;
using Application.Abstractions.Gateways;
using Application.UseCases.Queries.ListarOrdensServico;
using Domain.Entities;
using Domain.ValueObjects;
using Moq;

namespace UnitTests.Application.UseCases.Queries.ListarOrdensServico;

public class ListarOrdensServicoQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMappedResponses_InGatewayOrder()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem1 = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "A", Email = "a@teste.com" },
            new VeiculoOrdemServico { Placa = "AAA1111", Marca = "VW", Modelo = "Gol" });
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordem1, OrdemServicoTestIds.Of(1));
        ordem1.EnviarParaDiagnostico();

        var ordem2 = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 2, Nome = "B", Email = "b@teste.com" },
            new VeiculoOrdemServico { Placa = "BBB2222", Marca = "Fiat", Modelo = "Uno" });
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordem2, OrdemServicoTestIds.Of(2));

        gateway.Setup(g => g.ListarAtivasAsync()).ReturnsAsync([ordem1, ordem2]);
        var handler = new ListarOrdensServicoQueryHandler(gateway.Object);

        // Act
        var result = await handler.Handle(new ListarOrdensServicoQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(OrdemServicoTestIds.Of(1), result[0].Id);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, result[0].Status);
        Assert.Equal(OrdemServicoTestIds.Of(2), result[1].Id);
        Assert.Equal(StatusOrdemServico.Recebida, result[1].Status);
    }
}

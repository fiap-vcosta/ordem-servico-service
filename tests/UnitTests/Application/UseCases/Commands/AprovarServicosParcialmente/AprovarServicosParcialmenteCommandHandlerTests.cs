using Application.UseCases.Commands.AprovarServicosParcialmente;
using Domain.Exceptions;
using Application.Abstractions.Gateways;
using Domain.Entities;
using Domain.ValueObjects;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.Commands.AprovarServicosParcialmente;

public class AprovarServicosParcialmenteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ApprovesSelectedServices_WhenIdsAreValid()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();

        var ordem = CriarOrdemAguardandoAprovacaoComDoisServicos();
        var idPrimeiro = ordem.Servicos.First().Id;

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordem);
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AprovarServicosParcialmenteCommandHandler(mockOrdemServicoGateway.Object, NullLogger<AprovarServicosParcialmenteCommandHandler>.Instance);
        var command = new AprovarServicosParcialmenteCommand
        {
            IdOrdemServico = 1,
            IdServicosAprovados = [idPrimeiro]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.EmDiagnostico, result.Status);
        Assert.Equal(StatusItemOrdemServico.Aprovado, ordem.Servicos.First(s => s.Id == idPrimeiro).Status);
        mockOrdemServicoGateway.Verify(r => r.UpdateAsync(ordem), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_WhenServiceIdDoesNotBelongToOrdem()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemAguardandoAprovacaoComDoisServicos();

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordem);

        var handler = new AprovarServicosParcialmenteCommandHandler(mockOrdemServicoGateway.Object, NullLogger<AprovarServicosParcialmenteCommandHandler>.Instance);
        var command = new AprovarServicosParcialmenteCommand
        {
            IdOrdemServico = 1,
            IdServicosAprovados = [999]
        };

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenOrdemNotFound()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new AprovarServicosParcialmenteCommandHandler(mockOrdemServicoGateway.Object, NullLogger<AprovarServicosParcialmenteCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(new AprovarServicosParcialmenteCommand { IdOrdemServico = 999, IdServicosAprovados = [1] }, CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemAguardandoAprovacaoComDoisServicos()
    {
        var ordem = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" });
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordem, 1);

        var catalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Troca", 200m, catalogo, []);
        ordem.AdicionarItemServico("Alinhamento", 150m, catalogo, []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        return ordem;
    }
}

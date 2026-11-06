using Application.Abstractions.Gateways;
using Application.UseCases.OrdemServico.Commands.AplicarOrcamentoFalhou;
using Domain.Exceptions;
using Domain.OrdemServico.Entities;
using Domain.OrdemServico.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests.Application.UseCases.OrdemServico.Commands.AplicarOrcamentoFalhou;

public class AplicarOrcamentoFalhouCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAguardandoOrcamento_ReturnsToEmDiagnostico()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemAguardandoOrcamento();
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarOrcamentoFalhouCommandHandler(
            gateway.Object,
            NullLogger<AplicarOrcamentoFalhouCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoFalhouCommand { IdOrdemServico = 1, VersaoDiagnostico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.EmDiagnostico, result.Status);
        gateway.Verify(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWrongVersion_IsIdempotentNoOp()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemAguardandoOrcamento();
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);

        var handler = new AplicarOrcamentoFalhouCommandHandler(
            gateway.Object,
            NullLogger<AplicarOrcamentoFalhouCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarOrcamentoFalhouCommand { IdOrdemServico = 1, VersaoDiagnostico = 99 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoOrcamento, result.Status);
        gateway.Verify(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsDomainNotFoundException()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new AplicarOrcamentoFalhouCommandHandler(
            gateway.Object,
            NullLogger<AplicarOrcamentoFalhouCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(
                new AplicarOrcamentoFalhouCommand { IdOrdemServico = 999, VersaoDiagnostico = 1 },
                CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemAguardandoOrcamento()
    {
        var ordem = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" });
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico(
            "Troca",
            100m,
            new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" },
            []);
        ordem.FinalizarDiagnostico();
        typeof(OrdemServicoAggregateRoot).GetProperty("Id")!.SetValue(ordem, 1);
        return ordem;
    }
}

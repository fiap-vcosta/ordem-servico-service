using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarCobrancaGerada;
using Domain.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests.Application.UseCases.Commands.AplicarCobrancaGerada;

public class AplicarCobrancaGeradaCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenFinalizada_GoesToAguardandoPagamento()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemFinalizada();
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarCobrancaGeradaCommandHandler(
            gateway.Object,
            NullLogger<AplicarCobrancaGeradaCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarCobrancaGeradaCommand { IdOrdemServico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoPagamento, result.Status);
        gateway.Verify(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAlreadyAguardandoPagamento_IsIdempotent()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemFinalizada();
        ordem.RegistrarCobrancaGerada();
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarCobrancaGeradaCommandHandler(
            gateway.Object,
            NullLogger<AplicarCobrancaGeradaCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarCobrancaGeradaCommand { IdOrdemServico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoPagamento, result.Status);
        gateway.Verify(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCobrancaExpirada_GoesToAguardandoPagamento()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemFinalizada();
        ordem.RegistrarCobrancaGerada();
        ordem.RegistrarPagamentoRecusado(OrdemServicoAggregateRoot.MotivoPagamentoExpirado);
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarCobrancaGeradaCommandHandler(
            gateway.Object,
            NullLogger<AplicarCobrancaGeradaCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarCobrancaGeradaCommand { IdOrdemServico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoPagamento, result.Status);
        gateway.Verify(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWrongStatus_IsIdempotentNoOp()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" });
        typeof(OrdemServicoAggregateRoot).GetProperty("Id")!.SetValue(ordem, 1);
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);

        var handler = new AplicarCobrancaGeradaCommandHandler(
            gateway.Object,
            NullLogger<AplicarCobrancaGeradaCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarCobrancaGeradaCommand { IdOrdemServico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.Recebida, result.Status);
        gateway.Verify(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsDomainNotFoundException()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new AplicarCobrancaGeradaCommandHandler(
            gateway.Object,
            NullLogger<AplicarCobrancaGeradaCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(new AplicarCobrancaGeradaCommand { IdOrdemServico = 999 }, CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemFinalizada()
    {
        var ordem = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" });
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico(
            "Troca",
            100m,
            new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" },
            [
                new ItemNecessario.CriarItemNecessarioParams(1, 1m, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
            ]);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();
        ordem.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueTravado });
        var servicoId = ordem.Servicos.First().Id;
        ordem.ConfirmarExecucao(
        [
            new ServicoExecutado
            {
                IdServico = servicoId,
                IniciadoEm = DateTime.UtcNow.AddHours(-1),
                FinalizadoEm = DateTime.UtcNow
            }
        ]);
        typeof(OrdemServicoAggregateRoot).GetProperty("Id")!.SetValue(ordem, 1);
        return ordem;
    }
}

using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarPagamentoRecusado;
using Domain.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using Moq;

namespace UnitTests.Application.UseCases.Commands.AplicarPagamentoRecusado;

public class AplicarPagamentoRecusadoCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAguardandoPagamento_KeepsStatus()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemAguardandoPagamento();
        gateway.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(ordem);

        var handler = new AplicarPagamentoRecusadoCommandHandler(gateway.Object);

        // Act
        var result = await handler.Handle(
            new AplicarPagamentoRecusadoCommand { IdOrdemServico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoPagamento, result.Status);
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

        var handler = new AplicarPagamentoRecusadoCommandHandler(gateway.Object);

        // Act
        var result = await handler.Handle(
            new AplicarPagamentoRecusadoCommand { IdOrdemServico = 1 },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.Recebida, result.Status);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsDomainNotFoundException()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new AplicarPagamentoRecusadoCommandHandler(gateway.Object);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(new AplicarPagamentoRecusadoCommand { IdOrdemServico = 999 }, CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemAguardandoPagamento()
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
        ordem.ChecarItensNecessarios(new Dictionary<int, decimal> { [100] = 5m });
        ordem.TravarItensNecessarios();
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
        ordem.RegistrarCobrancaGerada();
        typeof(OrdemServicoAggregateRoot).GetProperty("Id")!.SetValue(ordem, 1);
        return ordem;
    }
}

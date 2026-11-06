using Application.UseCases.Estoque.ItemEstoque.Commands.ConfirmarUtilizacaoItensEstoque;
using Domain.Exceptions;
using Application.Abstractions.Gateways;
using Domain.OrdemServico.Entities;
using Domain.OrdemServico.ValueObjects;
using Domain.Estoque.Entities;
using Moq;

namespace UnitTests.Application.UseCases.Estoque.ItemEstoque.Commands.ConfirmarUtilizacaoItensEstoque;

public class ConfirmarUtilizacaoItensEstoqueCommandHandlerTests
{
    [Fact]
    public async Task Handle_ConfirmsUtilization_WhenOrdemExists()
    {
        // Arrange
        var mockOrdemGateway = new Mock<IOrdemServicoGateway>();
        var mockItemGateway = new Mock<IItemEstoqueGateway>();

        var ordem = CriarOrdemComItensTravados();
        var item = new ItemEstoqueAggregateRoot { Id = 100, Nome = "Pneu", Saldo = 10m, SaldoReservado = 4m };

        mockOrdemGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordem);
        mockItemGateway.Setup(r => r.GetUtilizadosByOrdemServico(1)).ReturnsAsync(new List<ItemEstoqueAggregateRoot> { item });
        mockItemGateway.Setup(r => r.UpdateAsync(It.IsAny<ItemEstoqueAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new ConfirmarUtilizacaoItensEstoqueCommandHandler(mockOrdemGateway.Object, mockItemGateway.Object);

        // Act
        await handler.Handle(new ConfirmarUtilizacaoItensEstoqueCommand { IdOrdemServico = 1 }, CancellationToken.None);

        // Assert
        Assert.Equal(6m, item.Saldo);
        Assert.Equal(0m, item.SaldoReservado);
        mockItemGateway.Verify(r => r.UpdateAsync(item), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_WhenOrdemNotFound()
    {
        // Arrange
        var mockOrdemGateway = new Mock<IOrdemServicoGateway>();
        mockOrdemGateway.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new ConfirmarUtilizacaoItensEstoqueCommandHandler(
            mockOrdemGateway.Object,
            new Mock<IItemEstoqueGateway>().Object);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(new ConfirmarUtilizacaoItensEstoqueCommand { IdOrdemServico = 999 }, CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemComItensTravados()
    {
        var ordem = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" });
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordem, 1);

        var catalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Troca", 200m, catalogo,
        [
            new ItemNecessario.CriarItemNecessarioParams(1, 4m, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
        ]);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();
        ordem.ChecarItensNecessarios(new Dictionary<int, decimal> { [100] = 10m });
        ordem.TravarItensNecessarios();
        return ordem;
    }
}

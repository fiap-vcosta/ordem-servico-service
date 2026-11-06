using Application.UseCases.Commands.AlocarEstoqueOrdemServico;
using Domain.Exceptions;
using Application.UseCases.Estoque.ItemEstoque.Commands.EnviarNotificacaoParaCompra;
using Application.UseCases.Estoque.ItemEstoque.Commands.TravarItensNecessarios;
using Domain.Estoque.Entities;
using Application.Abstractions.Gateways;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.Commands.AlocarEstoqueOrdemServico;

public class AlocarEstoqueOrdemServicoCommandHandlerTests
{
    [Fact]
    public async Task Handle_LocksStock_WhenStockIsSufficient()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockItemEstoqueGateway = new Mock<IItemEstoqueGateway>();
        var mockMediator = new Mock<IMediator>();

        var ordemServico = CriarOrdemEmChecandoEstoque(quantidadeNecessaria: 4m);
        ReflectSetId(ordemServico, 1);

        var itemEstoque = new ItemEstoqueAggregateRoot
        {
            Id = 100,
            Codigo = "PNU-001",
            Nome = "Pneu",
            Saldo = 10m,
            SaldoReservado = 0m
        };

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordemServico);
        mockItemEstoqueGateway
            .Setup(r => r.GetEBloquearItensAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ItemEstoqueAggregateRoot> { itemEstoque });
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        mockMediator
            .Setup(m => m.Send(It.IsAny<TravarItensNecessariosCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var handler = new AlocarEstoqueOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockItemEstoqueGateway.Object,
            mockMediator.Object,
            NullLogger<AlocarEstoqueOrdemServicoCommandHandler>.Instance);

        // Act
        await handler.Handle(new AlocarEstoqueOrdemServicoCommand(1), CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.LiberadaParaExecucao, ordemServico.Status);
        mockMediator.Verify(
            m => m.Send(It.Is<TravarItensNecessariosCommand>(c => c.IdItemEstoque == 100 && c.QuantidadeNecessaria == 4m), It.IsAny<CancellationToken>()),
            Times.Once);
        mockOrdemServicoGateway.Verify(r => r.UpdateAsync(ordemServico), Times.Once);
    }

    [Fact]
    public async Task Handle_RequestsPurchase_WhenStockIsInsufficient()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockItemEstoqueGateway = new Mock<IItemEstoqueGateway>();
        var mockMediator = new Mock<IMediator>();

        var ordemServico = CriarOrdemEmChecandoEstoque(quantidadeNecessaria: 10m);
        ReflectSetId(ordemServico, 1);

        var itemEstoque = new ItemEstoqueAggregateRoot
        {
            Id = 100,
            Codigo = "PNU-001",
            Nome = "Pneu",
            Saldo = 5m,
            SaldoReservado = 0m
        };

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordemServico);
        mockItemEstoqueGateway
            .Setup(r => r.GetEBloquearItensAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ItemEstoqueAggregateRoot> { itemEstoque });
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        mockMediator
            .Setup(m => m.Send(It.IsAny<EnviarNotificacaoParaCompraCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var handler = new AlocarEstoqueOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockItemEstoqueGateway.Object,
            mockMediator.Object,
            NullLogger<AlocarEstoqueOrdemServicoCommandHandler>.Instance);

        // Act
        await handler.Handle(new AlocarEstoqueOrdemServicoCommand(1), CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoPeca, ordemServico.Status);
        mockMediator.Verify(
            m => m.Send(
                It.Is<EnviarNotificacaoParaCompraCommand>(c =>
                    c.IdItemEstoque == 100 &&
                    c.IdOrdemServico == 1 &&
                    c.QuantidadeFaltando == 5m),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockOrdemServicoGateway.Verify(r => r.UpdateAsync(ordemServico), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsDomainNotFoundException_WhenOrdemServicoNotFound()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockItemEstoqueGateway = new Mock<IItemEstoqueGateway>();
        var mockMediator = new Mock<IMediator>();

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new AlocarEstoqueOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockItemEstoqueGateway.Object,
            mockMediator.Object,
            NullLogger<AlocarEstoqueOrdemServicoCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(new AlocarEstoqueOrdemServicoCommand(999), CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemEmChecandoEstoque(decimal quantidadeNecessaria)
    {
        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordem = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ReflectSetId(ordem, 1);

        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico(
            "Troca de pneus",
            200m,
            servicoCatalogo,
            new List<ItemNecessario.CriarItemNecessarioParams>
            {
                new(1, quantidadeNecessaria, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
            });
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();

        return ordem;
    }

    private static void ReflectSetId(OrdemServicoAggregateRoot ordem, int id)
    {
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordem, id);
    }
}

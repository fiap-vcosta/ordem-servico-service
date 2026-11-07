using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AdicionarItemOrdemServico;
using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;
using Moq;

namespace UnitTests.Application.UseCases.Commands.AdicionarItemOrdemServico;

public class AdicionarItemOrdemServicoCommandHandlerTests
{
    [Fact]
    public async Task Handle_AddsServiceItem_WhenCommandIsValid()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockCatalog = new Mock<IAdministrativoCatalogGateway>();
        var mockEstoqueCatalog = new Mock<IEstoqueCatalogGateway>();

        var ordem = CriarOrdemEmDiagnostico();
        var servico = new AdministrativoServicoDto(5, "OLE-001", "Troca de Óleo", 150m);
        var itemEstoque = new EstoqueItemDto(10, "FLT-001", "Filtro", "Unidade");

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordem);
        mockCatalog.Setup(r => r.GetServicoByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(servico);
        mockEstoqueCatalog.Setup(r => r.GetItemByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(itemEstoque);
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AdicionarItemOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockCatalog.Object,
            mockEstoqueCatalog.Object);

        var command = new AdicionarItemOrdemServicoCommand
        {
            IdOrdemServico = 1,
            IdServico = 5,
            ValorCobrado = 1000m,
            ItensNecessarios =
            [
                new AdicionarItemOrdemServicoCommand.ItemNecessario { IdItemEstoque = 10, Quantidade = 2m }
            ]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Itens);
        Assert.Equal(1000m, result.ValorTotal);
        mockOrdemServicoGateway.Verify(r => r.UpdateAsync(ordem), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_WhenOrdemServicoNotFound()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new AdicionarItemOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            new Mock<IAdministrativoCatalogGateway>().Object,
            new Mock<IEstoqueCatalogGateway>().Object);

        var command = new AdicionarItemOrdemServicoCommand { IdOrdemServico = 999, IdServico = 1, ValorCobrado = 100m };

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenServicoNotFound()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockCatalog = new Mock<IAdministrativoCatalogGateway>();

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CriarOrdemEmDiagnostico());
        mockCatalog.Setup(r => r.GetServicoByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((AdministrativoServicoDto?)null);

        var handler = new AdicionarItemOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockCatalog.Object,
            new Mock<IEstoqueCatalogGateway>().Object);

        var command = new AdicionarItemOrdemServicoCommand { IdOrdemServico = 1, IdServico = 999, ValorCobrado = 100m };

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenItemEstoqueNotFound()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockCatalog = new Mock<IAdministrativoCatalogGateway>();
        var mockEstoqueCatalog = new Mock<IEstoqueCatalogGateway>();

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CriarOrdemEmDiagnostico());
        mockCatalog.Setup(r => r.GetServicoByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdministrativoServicoDto(5, "OLE-001", "Troca de Óleo", 150m));
        mockEstoqueCatalog.Setup(r => r.GetItemByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EstoqueItemDto?)null);

        var handler = new AdicionarItemOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockCatalog.Object,
            mockEstoqueCatalog.Object);

        var command = new AdicionarItemOrdemServicoCommand
        {
            IdOrdemServico = 1,
            IdServico = 5,
            ValorCobrado = 100m,
            ItensNecessarios =
            [
                new AdicionarItemOrdemServicoCommand.ItemNecessario { IdItemEstoque = 10, Quantidade = 1m }
            ]
        };

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemEmDiagnostico()
    {
        var ordem = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com", Documento = "71284693031" },
            new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" });
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordem, 1);
        ordem.EnviarParaDiagnostico();
        return ordem;
    }
}

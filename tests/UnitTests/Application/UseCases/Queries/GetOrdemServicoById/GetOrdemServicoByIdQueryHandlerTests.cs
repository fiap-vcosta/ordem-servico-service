using UnitTests.Support;
using Application.UseCases.Queries.GetOrdemServicoById;
using Application.Abstractions.Gateways;
using Domain.Entities;
using Domain.ValueObjects;
using Moq;

namespace UnitTests.Application.UseCases.Queries.GetOrdemServicoById;

public class GetOrdemServicoByIdQueryHandlerTests
{
    private readonly Mock<IOrdemServicoGateway> _mockOrdemServicoGateway;
    private readonly GetOrdemServicoByIdQueryHandler _handler;

    public GetOrdemServicoByIdQueryHandlerTests()
    {
        _mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        _handler = new GetOrdemServicoByIdQueryHandler(_mockOrdemServicoGateway.Object);
    }

    [Fact]
    public async Task Handle_ReturnsOrdemServico_WhenExists()
    {
        // Arrange
        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordemServico = OrdemServicoAggregateRoot.Criar(cliente, veiculo);

        var query = new GetOrdemServicoByIdQuery { Id = OrdemServicoTestIds.Of(1) };

        _mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordemServico);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cliente.Nome, result.Cliente.Nome);
        _mockOrdemServicoGateway.Verify(r => r.GetByIdAsync(OrdemServicoTestIds.Of(1)), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenOrdemServicoNotFound()
    {
        // Arrange
        var query = new GetOrdemServicoByIdQuery { Id = OrdemServicoTestIds.Of(999) };
        _mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(OrdemServicoTestIds.Of(999))).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _mockOrdemServicoGateway.Verify(r => r.GetByIdAsync(OrdemServicoTestIds.Of(999)), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsOrdemServicoWithServices_WhenHasServices()
    {
        // Arrange
        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordemServico = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ordemServico.EnviarParaDiagnostico();
        
        var servicoCatalogo = new ServicoCatalogo() { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordemServico.AdicionarItemServico("Troca de pneus", 200m, servicoCatalogo, new List<ItemNecessario.CriarItemNecessarioParams>());

        var query = new GetOrdemServicoByIdQuery { Id = OrdemServicoTestIds.Of(1) };

        _mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordemServico);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Servicos);
        _mockOrdemServicoGateway.Verify(r => r.GetByIdAsync(OrdemServicoTestIds.Of(1)), Times.Once);
    }
}

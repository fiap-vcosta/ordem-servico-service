using Application.UseCases.Queries.GetTempoMedioAllServicos;
using Application.Abstractions.Gateways;
using Moq;

namespace UnitTests.Application.UseCases.Queries.GetTempoMedioAllServicos;

public class GetTempoMedioExecucaoAllServicosQueryHandlerTests
{
    private readonly Mock<IItemServicoGateway> _mockItemServicoGateway;
    private readonly GetTempoMedioExecucaoAllServicosQueryHandler _handler;

    public GetTempoMedioExecucaoAllServicosQueryHandlerTests()
    {
        _mockItemServicoGateway = new Mock<IItemServicoGateway>();
        _handler = new GetTempoMedioExecucaoAllServicosQueryHandler(_mockItemServicoGateway.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAverageExecutionTimes_WhenServicesExist()
    {
        // Arrange
        var temposMedios = new List<IItemServicoGateway.TempoMedioExecucaoServico>
        {
            new(1, 5, TimeSpan.FromHours(2)),
            new(2, 3, TimeSpan.FromHours(1)),
            new(3, 8, TimeSpan.FromHours(3))
        };

        var query = new GetTempoMedioExecucaoAllServicosQuery();

        _mockItemServicoGateway
            .Setup(r => r.GetAllTempoMedioExecucaoAsync())
            .ReturnsAsync(temposMedios);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, t => t.IdServico == 1 && t.ExecucaoMedia == TimeSpan.FromHours(2));
        _mockItemServicoGateway.Verify(r => r.GetAllTempoMedioExecucaoAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoServicesExist()
    {
        // Arrange
        var query = new GetTempoMedioExecucaoAllServicosQuery();

        _mockItemServicoGateway
            .Setup(r => r.GetAllTempoMedioExecucaoAsync())
            .ReturnsAsync(new List<IItemServicoGateway.TempoMedioExecucaoServico>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockItemServicoGateway.Verify(r => r.GetAllTempoMedioExecucaoAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsDifferentAverageExecutionTimes()
    {
        // Arrange
        var temposMedios = new List<IItemServicoGateway.TempoMedioExecucaoServico>
        {
            new(1, 10, TimeSpan.FromMinutes(30)),
            new(2, 2, TimeSpan.FromHours(8))
        };

        var query = new GetTempoMedioExecucaoAllServicosQuery();

        _mockItemServicoGateway
            .Setup(r => r.GetAllTempoMedioExecucaoAsync())
            .ReturnsAsync(temposMedios);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        var serviceRapido = result.First(t => t.IdServico == 1);
        var servicoLongo = result.First(t => t.IdServico == 2);
        Assert.Equal(TimeSpan.FromMinutes(30), serviceRapido.ExecucaoMedia);
        Assert.Equal(TimeSpan.FromHours(8), servicoLongo.ExecucaoMedia);
    }
}

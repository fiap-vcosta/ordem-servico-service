using Application.Abstractions.Messaging;
using Application.UseCases.Commands.ConfirmarExecucaoOrdemServico;
using Application.Abstractions.Gateways;
using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.Commands.ConfirmarExecucaoOrdemServico;

public class ConfirmarExecucaoOrdemServicoCommandHandlerTests
{
    [Fact]
    public async Task Handle_Throws_WhenServicoNaoPertenceAOrdem()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockPublisher = new Mock<IOrdemServicoIntegrationPublisher>();

        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordemServico = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ordemServico.EnviarParaDiagnostico();

        var servicoCatalogo = new ServicoCatalogo() { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordemServico.AdicionarItemServico("Troca", 100m, servicoCatalogo, new List<ItemNecessario.CriarItemNecessarioParams>());
        ordemServico.FinalizarDiagnostico();
        ordemServico.AplicarOrcamentoGerado(1);
        ordemServico.AprovarServicosSugeridos();
        ordemServico.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque>());

        var command = new ConfirmarExecucaoOrdemServicoCommand
        {
            IdOrdemServico = 1,
            ServicosExecutados =
            [
                new ServicoExecutado
                {
                    IdServico = 999,
                    IniciadoEm = DateTime.UtcNow.AddHours(-1),
                    FinalizadoEm = DateTime.UtcNow
                }
            ]
        };

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordemServico);

        var handler = new ConfirmarExecucaoOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockPublisher.Object,
            NullLogger<ConfirmarExecucaoOrdemServicoCommandHandler>.Instance);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<DomainNotFoundException>(act);
        Assert.Contains("999", ex.Message, StringComparison.Ordinal);
        mockOrdemServicoGateway.Verify(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Never);
        mockPublisher.Verify(
            p => p.PublishOrdemServicoFinalizadaAsync(It.IsAny<OrdemServicoAggregateRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConfirmsExecution_WhenAllServicesComplete()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockPublisher = new Mock<IOrdemServicoIntegrationPublisher>();

        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordemServico = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ordemServico.EnviarParaDiagnostico();

        var servicoCatalogo = new ServicoCatalogo() { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordemServico.AdicionarItemServico("Troca", 100m, servicoCatalogo, new List<ItemNecessario.CriarItemNecessarioParams>());
        ordemServico.FinalizarDiagnostico();
        ordemServico.AplicarOrcamentoGerado(1);
        ordemServico.AprovarServicosSugeridos();
        ordemServico.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque>());

        var servicoId = ordemServico.Servicos.First().Id;

        var command = new ConfirmarExecucaoOrdemServicoCommand
        {
            IdOrdemServico = 1,
            ServicosExecutados = new List<ServicoExecutado>
            {
                new() { IdServico = servicoId, IniciadoEm = DateTime.UtcNow.AddHours(-1), FinalizadoEm = DateTime.UtcNow }
            }
        };

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordemServico);
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        mockPublisher
            .Setup(p => p.PublishOrdemServicoFinalizadaAsync(It.IsAny<OrdemServicoAggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new ConfirmarExecucaoOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockPublisher.Object,
            NullLogger<ConfirmarExecucaoOrdemServicoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusOrdemServico.Finalizada, result.Status);
        mockPublisher.Verify(
            p => p.PublishOrdemServicoFinalizadaAsync(ordemServico, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

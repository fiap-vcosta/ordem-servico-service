using UnitTests.Support;
using Application.Abstractions.Messaging;
using Application.UseCases.Commands.AprovarOrdemServico;
using Application.Abstractions.Gateways;
using Domain.Entities;
using Domain.ValueObjects;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.Commands.AprovarOrdemServico;

public class AprovarOrdemServicoCommandHandlerTests
{
    [Fact]
    public async Task Handle_ApprovesOrdemServico_WhenCommandIsValid()
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

        var command = new AprovarOrdemServicoCommand { IdOrdemServico = OrdemServicoTestIds.Of(1) };

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordemServico);
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        mockPublisher
            .Setup(p => p.PublishOrdemServicoAprovadaAsync(It.IsAny<OrdemServicoAggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new AprovarOrdemServicoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockPublisher.Object,
            NullLogger<AprovarOrdemServicoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusOrdemServico.ChecandoEstoque, result.Status);
        mockPublisher.Verify(
            p => p.PublishOrdemServicoAprovadaAsync(ordemServico, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

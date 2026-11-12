using UnitTests.Support;
using Application.Abstractions.Gateways;
using Application.Abstractions.Messaging;
using Application.UseCases.Commands.FinalizarDiagnostico;
using Domain.Entities;
using Domain.ValueObjects;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.Commands.FinalizarDiagnostico;

public class FinalizarDiagnosticoCommandHandlerTests
{
    [Fact]
    public async Task Handle_FinalizesToAguardandoOrcamentoAndPublishes()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockPublisher = new Mock<IOrdemServicoIntegrationPublisher>();

        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordemServico = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ordemServico.EnviarParaDiagnostico();

        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordemServico.AdicionarItemServico("Troca", 100m, servicoCatalogo, []);

        var command = new FinalizarDiagnosticoCommand { IdOrdemServico = OrdemServicoTestIds.Of(1) };

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordemServico);
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        mockPublisher
            .Setup(p => p.PublishDiagnosticoFinalizadoAsync(It.IsAny<OrdemServicoAggregateRoot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new FinalizarDiagnosticoCommandHandler(
            mockOrdemServicoGateway.Object,
            mockPublisher.Object,
            NullLogger<FinalizarDiagnosticoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusOrdemServico.AguardandoOrcamento, result.Status);
        mockPublisher.Verify(
            p => p.PublishDiagnosticoFinalizadoAsync(ordemServico, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

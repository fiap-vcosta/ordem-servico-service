using UnitTests.Support;
using Application.UseCases.Commands.RejeitarOrdemServico;
using Application.Abstractions.Gateways;
using Domain.Entities;
using Domain.ValueObjects;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.Commands.RejeitarOrdemServico;

public class RejeitarOrdemServicoCommandHandlerTests
{
    [Fact]
    public async Task Handle_RejectsOrdemServico_WhenCommandIsValid()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();

        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordemServico = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ordemServico.EnviarParaDiagnostico();
        
        var servicoCatalogo = new ServicoCatalogo() { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordemServico.AdicionarItemServico("Troca", 100m, servicoCatalogo, new List<ItemNecessario.CriarItemNecessarioParams>());
        ordemServico.FinalizarDiagnostico();
        ordemServico.AplicarOrcamentoGerado(1);

        var command = new RejeitarOrdemServicoCommand { IdOrdemServico = OrdemServicoTestIds.Of(1) };

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordemServico);
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new RejeitarOrdemServicoCommandHandler(mockOrdemServicoGateway.Object, NullLogger<RejeitarOrdemServicoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, result.Status);
    }
}

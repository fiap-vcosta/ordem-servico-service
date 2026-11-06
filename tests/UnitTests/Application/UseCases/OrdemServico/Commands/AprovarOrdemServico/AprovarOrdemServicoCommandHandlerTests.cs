using Application.Abstractions.Events;
using Application.UseCases.OrdemServico.Commands.AprovarOrdemServico;
using Application.Abstractions.Gateways;
using Domain.OrdemServico.Entities;
using Domain.OrdemServico.Events;
using Domain.OrdemServico.ValueObjects;
using MediatR;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.OrdemServico.Commands.AprovarOrdemServico;

public class AprovarOrdemServicoCommandHandlerTests
{
    [Fact]
    public async Task Handle_ApprovesOrdemServico_WhenCommandIsValid()
    {
        // Arrange
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockMediator = new Mock<IMediator>();

        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        var ordemServico = OrdemServicoAggregateRoot.Criar(cliente, veiculo);
        ordemServico.EnviarParaDiagnostico();
        
        var servicoCatalogo = new ServicoCatalogo() { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordemServico.AdicionarItemServico("Troca", 100m, servicoCatalogo, new List<ItemNecessario.CriarItemNecessarioParams>());
        ordemServico.FinalizarDiagnostico();
        ordemServico.AplicarOrcamentoGerado(1);

        var command = new AprovarOrdemServicoCommand { IdOrdemServico = 1 };

        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ordemServico);
        mockOrdemServicoGateway.Setup(r => r.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);
        mockMediator.Setup(m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoAprovadaEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new AprovarOrdemServicoCommandHandler(mockOrdemServicoGateway.Object, mockMediator.Object,
            NullLogger<AprovarOrdemServicoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusOrdemServico.ChecandoEstoque, result.Status);
    }
}

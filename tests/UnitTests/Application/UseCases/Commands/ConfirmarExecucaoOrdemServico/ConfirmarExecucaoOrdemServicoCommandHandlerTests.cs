using Application.UseCases.Commands.ConfirmarExecucaoOrdemServico;
using Application.Abstractions.Gateways;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.UseCases.Commands.ConfirmarExecucaoOrdemServico;

public class ConfirmarExecucaoOrdemServicoCommandHandlerTests
{
    [Fact]
    public async Task Handle_ConfirmsExecution_WhenAllServicesComplete()
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
        ordemServico.AprovarServicosSugeridos();
        ordemServico.ChecarItensNecessarios(new Dictionary<int, decimal>());

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
        mockMediator.Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));

        var handler = new ConfirmarExecucaoOrdemServicoCommandHandler(mockOrdemServicoGateway.Object, mockMediator.Object,
            NullLogger<ConfirmarExecucaoOrdemServicoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusOrdemServico.Finalizada, result.Status);
    }
}

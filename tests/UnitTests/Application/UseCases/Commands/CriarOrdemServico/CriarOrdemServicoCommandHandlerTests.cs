using Application.Abstractions.Events;
using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AdicionarItemOrdemServico;
using Application.UseCases.Commands.CriarOrdemServico;
using Application.UseCases.Responses;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests.Application.UseCases.Commands.CriarOrdemServico;

public class CriarOrdemServicoCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesOrdemServico_WhenCommandIsValid()
    {
        // Arrange
        var mockCatalog = new Mock<IAdministrativoCatalogGateway>();
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockMediator = new Mock<IMediator>();

        var cliente = new AdministrativoClienteDto(1, "João", "joao@test.com", "12345678901");
        var veiculo = new AdministrativoVeiculoDto(1, 1, "ABC-1234", "Toyota", "Corolla");
        var command = new CriarOrdemServicoCommand { IdVeiculo = 1 };

        mockCatalog.Setup(r => r.GetVeiculoByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);
        mockCatalog.Setup(r => r.GetClienteByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        mockOrdemServicoGateway
            .Setup(r => r.CriarAsync(It.IsAny<OrdemServicoAggregateRoot>()))
            .Callback<OrdemServicoAggregateRoot>(os =>
                typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(os, 10))
            .Returns(Task.CompletedTask);

        var ordemPersistida = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "João", Email = "joao@test.com", Documento = "12345678901" },
            new VeiculoOrdemServico { Placa = "ABC-1234", Marca = "Toyota", Modelo = "Corolla" });
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordemPersistida, 10);
        ordemPersistida.EnviarParaDiagnostico();
        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(ordemPersistida);
        mockMediator
            .Setup(m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoCriadaEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CriarOrdemServicoCommandHandler(
            mockCatalog.Object,
            mockOrdemServicoGateway.Object,
            mockMediator.Object,
            NullLogger<CriarOrdemServicoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, result.Status);
        Assert.Equal(cliente.Nome, result.Cliente.Nome);
        Assert.Equal(veiculo.Placa, result.Veiculo.Placa);
        Assert.Empty(result.Servicos);
        mockOrdemServicoGateway.Verify(r => r.CriarAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Once);
        mockMediator.Verify(
            m => m.Send(It.IsAny<AdicionarItemOrdemServicoCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
        mockMediator.Verify(
            m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoCriadaEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SendsAdicionarItemCommands_WhenServicosProvided()
    {
        // Arrange
        var mockCatalog = new Mock<IAdministrativoCatalogGateway>();
        var mockOrdemServicoGateway = new Mock<IOrdemServicoGateway>();
        var mockMediator = new Mock<IMediator>();

        var cliente = new AdministrativoClienteDto(1, "João", "joao@test.com", "12345678901");
        var veiculo = new AdministrativoVeiculoDto(1, 1, "ABC-1234", "Toyota", "Corolla");

        mockCatalog.Setup(r => r.GetVeiculoByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);
        mockCatalog.Setup(r => r.GetClienteByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        mockOrdemServicoGateway
            .Setup(r => r.CriarAsync(It.IsAny<OrdemServicoAggregateRoot>()))
            .Callback<OrdemServicoAggregateRoot>(os =>
                typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(os, 20))
            .Returns(Task.CompletedTask);

        var ordemPersistida = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "João", Email = "joao@test.com", Documento = "12345678901" },
            new VeiculoOrdemServico { Placa = "ABC-1234", Marca = "Toyota", Modelo = "Corolla" });
        typeof(OrdemServicoAggregateRoot).GetProperty(nameof(OrdemServicoAggregateRoot.Id))!.SetValue(ordemPersistida, 20);
        ordemPersistida.EnviarParaDiagnostico();
        ordemPersistida.AdicionarItemServico(
            "Troca de Óleo",
            150m,
            new ServicoCatalogo { Id = 1, Nome = "Troca de Óleo", Codigo = "MTR-001" },
            []);
        mockOrdemServicoGateway.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(ordemPersistida);

        mockMediator
            .Setup(m => m.Send(It.IsAny<AdicionarItemOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdicionarItemOrdemServicoCommandResponse
            {
                Id = 20,
                Status = StatusOrdemServico.Recebida,
                ValorTotal = 150m,
                RecebidaEm = DateTime.UtcNow,
                Cliente = ClienteOrdemServicoResponse.From(ordemPersistida.Cliente),
                Veiculo = VeiculoOrdemServicoResponse.From(ordemPersistida.Veiculo),
                Itens = []
            });
        mockMediator
            .Setup(m => m.Publish(It.IsAny<DomainEventNotification<OrdemServicoCriadaEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CriarOrdemServicoCommandHandler(
            mockCatalog.Object,
            mockOrdemServicoGateway.Object,
            mockMediator.Object,
            NullLogger<CriarOrdemServicoCommandHandler>.Instance);

        var command = new CriarOrdemServicoCommand
        {
            IdVeiculo = 1,
            Servicos =
            [
                new CriarOrdemServicoCommand.Servico
                {
                    IdServico = 1,
                    ValorCobrado = 150m,
                    ItensNecessarios =
                    [
                        new CriarOrdemServicoCommand.ItemNecessario { IdItemEstoque = 1, Quantidade = 2m }
                    ]
                }
            ]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.EmDiagnostico, result.Status);
        Assert.Single(result.Servicos);
        mockMediator.Verify(
            m => m.Send(
                It.Is<AdicionarItemOrdemServicoCommand>(c =>
                    c.IdOrdemServico == 20 &&
                    c.IdServico == 1 &&
                    c.ValorCobrado == 150m &&
                    c.ItensNecessarios.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

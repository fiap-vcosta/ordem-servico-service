using UnitTests.Support;
using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarEstoqueReservado;
using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests.Application.UseCases.Commands.AplicarEstoqueReservado;

public class AplicarEstoqueReservadoCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenChecandoEstoque_AppliesReservation()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemChecandoEstoqueComItem(100);
        var aprovadaEm = new DateTimeOffset(DateTime.SpecifyKind(ordem.AprovadaEm!.Value, DateTimeKind.Utc));
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarEstoqueReservadoCommandHandler(
            gateway.Object,
            NullLogger<AplicarEstoqueReservadoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarEstoqueReservadoCommand
            {
                IdOrdemServico = OrdemServicoTestIds.Of(1),
                AprovadaEm = aprovadaEm,
                Itens = [new AplicarEstoqueItemStatus(100, StatusItemEstoque.EstoqueTravado)]
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.LiberadaParaExecucao, result.Status);
        gateway.Verify(g => g.UpdateAsync(ordem), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAprovadaEmMismatch_IsIdempotentNoOp()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemChecandoEstoqueComItem(100);
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordem);

        var handler = new AplicarEstoqueReservadoCommandHandler(
            gateway.Object,
            NullLogger<AplicarEstoqueReservadoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarEstoqueReservadoCommand
            {
                IdOrdemServico = OrdemServicoTestIds.Of(1),
                AprovadaEm = DateTimeOffset.UtcNow.AddHours(-1),
                Itens = [new AplicarEstoqueItemStatus(100, StatusItemEstoque.EstoqueTravado)]
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.ChecandoEstoque, result.Status);
        gateway.Verify(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAprovadaEmDiffersOnlyBySubMicrosecond_AppliesReservation()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        var ordem = CriarOrdemChecandoEstoqueComItem(100);
        var persistida = new DateTime(
            ordem.AprovadaEm!.Value.Ticks - (ordem.AprovadaEm.Value.Ticks % TimeSpan.TicksPerMicrosecond),
            DateTimeKind.Utc);
        typeof(OrdemServicoAggregateRoot).GetProperty("AprovadaEm")!.SetValue(ordem, persistida);
        var evento = new DateTimeOffset(persistida.AddTicks(7), TimeSpan.Zero);
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(1))).ReturnsAsync(ordem);
        gateway.Setup(g => g.UpdateAsync(It.IsAny<OrdemServicoAggregateRoot>())).Returns(Task.CompletedTask);

        var handler = new AplicarEstoqueReservadoCommandHandler(
            gateway.Object,
            NullLogger<AplicarEstoqueReservadoCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new AplicarEstoqueReservadoCommand
            {
                IdOrdemServico = OrdemServicoTestIds.Of(1),
                AprovadaEm = evento,
                Itens = [new AplicarEstoqueItemStatus(100, StatusItemEstoque.EstoqueTravado)]
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusOrdemServico.LiberadaParaExecucao, result.Status);
        gateway.Verify(g => g.UpdateAsync(ordem), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsDomainNotFoundException()
    {
        // Arrange
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.GetByIdAsync(OrdemServicoTestIds.Of(999))).ReturnsAsync((OrdemServicoAggregateRoot?)null);

        var handler = new AplicarEstoqueReservadoCommandHandler(
            gateway.Object,
            NullLogger<AplicarEstoqueReservadoCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(
                new AplicarEstoqueReservadoCommand
                {
                    IdOrdemServico = OrdemServicoTestIds.Of(999),
                    AprovadaEm = DateTimeOffset.UtcNow,
                    Itens = []
                },
                CancellationToken.None));
    }

    private static OrdemServicoAggregateRoot CriarOrdemChecandoEstoqueComItem(int itemId)
    {
        var ordem = OrdemServicoAggregateRoot.Criar(
            new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" },
            new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" });
        typeof(OrdemServicoAggregateRoot).GetProperty("Id")!.SetValue(ordem, OrdemServicoTestIds.Of(1));
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico(
            "Troca",
            100m,
            new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" },
            [new ItemNecessario.CriarItemNecessarioParams(OrdemServicoTestIds.Of(1), 1m, new ItemEstoqueOrdemServico { Id = itemId, Nome = "Pneu" })]);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();
        return ordem;
    }
}

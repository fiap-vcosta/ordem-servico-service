using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarEstoqueReservado;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

[ExcludeFromCodeCoverage(Justification = "Thin DI adapter; covered via AplicarEstoqueReservado handler tests.")]
public class EstoqueReservadoConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<EstoqueReservadoData>
{
    public string EventType => EventTypes.EstoqueReservado;

    public async Task HandleAsync(EventEnvelope<EstoqueReservadoData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IEventoProcessadoGateway>();
        var chave = $"{envelope.Data.OrdemServicoId}:{envelope.Data.AprovadaEm:O}";
        if (await IntegrationEventIdempotency.AlreadyProcessedAsync(
                gateway, envelope.EventId, EventTypes.EstoqueReservado, chave, cancellationToken))
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(
            new AplicarEstoqueReservadoCommand
            {
                IdOrdemServico = envelope.Data.OrdemServicoId,
                AprovadaEm = envelope.Data.AprovadaEm,
                Itens = envelope.Data.Itens
                    .Select(i => new AplicarEstoqueItemStatus(i.ItemEstoqueId, MapStatus(i.Status)))
                    .ToList()
            },
            cancellationToken);

        await IntegrationEventIdempotency.MarkProcessedAsync(
            gateway, envelope.EventId, EventTypes.EstoqueReservado, chave, cancellationToken);
    }

    private static StatusItemEstoque MapStatus(ItemEstoqueEventoStatus status) =>
        status switch
        {
            ItemEstoqueEventoStatus.EstoqueTravado => StatusItemEstoque.EstoqueTravado,
            ItemEstoqueEventoStatus.EstoqueDisponivel => StatusItemEstoque.EstoqueDisponivel,
            ItemEstoqueEventoStatus.EstoqueEmFalta => StatusItemEstoque.EstoqueEmFalta,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}

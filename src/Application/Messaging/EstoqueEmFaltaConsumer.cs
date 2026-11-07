using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarEstoqueEmFalta;
using Application.UseCases.Commands.AplicarEstoqueReservado;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

[ExcludeFromCodeCoverage(Justification = "Thin DI adapter; covered via AplicarEstoqueEmFalta handler tests.")]
public class EstoqueEmFaltaConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<EstoqueEmFaltaData>
{
    public string EventType => EventTypes.EstoqueEmFalta;

    public async Task HandleAsync(EventEnvelope<EstoqueEmFaltaData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IEventoProcessadoGateway>();
        var chave = $"{envelope.Data.OrdemServicoId}:{envelope.Data.AprovadaEm:O}";
        if (await IntegrationEventIdempotency.AlreadyProcessedAsync(
                gateway, envelope.EventId, EventTypes.EstoqueEmFalta, chave, cancellationToken))
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(
            new AplicarEstoqueEmFaltaCommand
            {
                IdOrdemServico = envelope.Data.OrdemServicoId,
                AprovadaEm = envelope.Data.AprovadaEm,
                Itens = envelope.Data.Itens
                    .Select(i => new AplicarEstoqueItemStatus(i.ItemEstoqueId, MapStatus(i.Status)))
                    .ToList()
            },
            cancellationToken);

        await IntegrationEventIdempotency.MarkProcessedAsync(
            gateway, envelope.EventId, EventTypes.EstoqueEmFalta, chave, cancellationToken);
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

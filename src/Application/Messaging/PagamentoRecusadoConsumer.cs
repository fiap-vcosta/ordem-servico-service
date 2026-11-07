using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarPagamentoRecusado;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

[ExcludeFromCodeCoverage(Justification = "Thin DI adapter; covered via AplicarPagamentoRecusado handler tests.")]
public class PagamentoRecusadoConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<PagamentoRecusadoData>
{
    public string EventType => EventTypes.PagamentoRecusado;

    public async Task HandleAsync(EventEnvelope<PagamentoRecusadoData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IEventoProcessadoGateway>();
        var chave = $"{envelope.Data.OrdemServicoId}:{envelope.Data.CobrancaId}";
        if (await IntegrationEventIdempotency.AlreadyProcessedAsync(
                gateway, envelope.EventId, EventTypes.PagamentoRecusado, chave, cancellationToken))
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(
            new AplicarPagamentoRecusadoCommand { IdOrdemServico = envelope.Data.OrdemServicoId },
            cancellationToken);

        await IntegrationEventIdempotency.MarkProcessedAsync(
            gateway, envelope.EventId, EventTypes.PagamentoRecusado, chave, cancellationToken);
    }
}

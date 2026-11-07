using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarPagamentoConfirmado;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

[ExcludeFromCodeCoverage(Justification = "Thin DI adapter; covered via AplicarPagamentoConfirmado handler tests.")]
public class PagamentoConfirmadoConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<PagamentoConfirmadoData>
{
    public string EventType => EventTypes.PagamentoConfirmado;

    public async Task HandleAsync(EventEnvelope<PagamentoConfirmadoData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IEventoProcessadoGateway>();
        var chave = $"{envelope.Data.OrdemServicoId}:{envelope.Data.CobrancaId}";
        if (await IntegrationEventIdempotency.AlreadyProcessedAsync(
                gateway, envelope.EventId, EventTypes.PagamentoConfirmado, chave, cancellationToken))
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(
            new AplicarPagamentoConfirmadoCommand { IdOrdemServico = envelope.Data.OrdemServicoId },
            cancellationToken);

        await IntegrationEventIdempotency.MarkProcessedAsync(
            gateway, envelope.EventId, EventTypes.PagamentoConfirmado, chave, cancellationToken);
    }
}

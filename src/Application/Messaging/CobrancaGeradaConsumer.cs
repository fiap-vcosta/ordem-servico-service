using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarCobrancaGerada;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

[ExcludeFromCodeCoverage(Justification = "Thin DI adapter; covered via AplicarCobrancaGerada handler tests.")]
public class CobrancaGeradaConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<CobrancaGeradaData>
{
    public string EventType => EventTypes.CobrancaGerada;

    public async Task HandleAsync(EventEnvelope<CobrancaGeradaData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IEventoProcessadoGateway>();
        var chave = $"{envelope.Data.OrdemServicoId}:{envelope.Data.CobrancaId}";
        if (await IntegrationEventIdempotency.AlreadyProcessedAsync(
                gateway, envelope.EventId, EventTypes.CobrancaGerada, chave, cancellationToken))
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(
            new AplicarCobrancaGeradaCommand { IdOrdemServico = envelope.Data.OrdemServicoId },
            cancellationToken);

        await IntegrationEventIdempotency.MarkProcessedAsync(
            gateway, envelope.EventId, EventTypes.CobrancaGerada, chave, cancellationToken);
    }
}

using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarOrcamentoFalhou;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

[ExcludeFromCodeCoverage(Justification = "Thin DI adapter; covered via AplicarOrcamentoFalhou handler tests.")]
public class OrcamentoFalhouConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<OrcamentoFalhouData>
{
    public string EventType => EventTypes.OrcamentoFalhou;

    public async Task HandleAsync(EventEnvelope<OrcamentoFalhouData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IEventoProcessadoGateway>();
        var chave = $"{envelope.Data.OrdemServicoId}:{envelope.Data.VersaoDiagnostico}";
        if (await IntegrationEventIdempotency.AlreadyProcessedAsync(
                gateway, envelope.EventId, EventTypes.OrcamentoFalhou, chave, cancellationToken))
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(
            new AplicarOrcamentoFalhouCommand
            {
                IdOrdemServico = envelope.Data.OrdemServicoId,
                VersaoDiagnostico = envelope.Data.VersaoDiagnostico
            },
            cancellationToken);

        await IntegrationEventIdempotency.MarkProcessedAsync(
            gateway, envelope.EventId, EventTypes.OrcamentoFalhou, chave, cancellationToken);
    }
}

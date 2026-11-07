using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Gateways;
using Application.UseCases.Commands.AplicarOrcamentoGerado;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

[ExcludeFromCodeCoverage(Justification = "Thin DI adapter; covered via AplicarOrcamentoGerado handler tests.")]
public class OrcamentoGeradoConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<OrcamentoGeradoData>
{
    public string EventType => EventTypes.OrcamentoGerado;

    public async Task HandleAsync(EventEnvelope<OrcamentoGeradoData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IEventoProcessadoGateway>();
        var chave = $"{envelope.Data.OrdemServicoId}:{envelope.Data.VersaoDiagnostico}";
        if (await IntegrationEventIdempotency.AlreadyProcessedAsync(
                gateway, envelope.EventId, EventTypes.OrcamentoGerado, chave, cancellationToken))
        {
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(
            new AplicarOrcamentoGeradoCommand
            {
                IdOrdemServico = envelope.Data.OrdemServicoId,
                VersaoDiagnostico = envelope.Data.VersaoDiagnostico
            },
            cancellationToken);

        await IntegrationEventIdempotency.MarkProcessedAsync(
            gateway, envelope.EventId, EventTypes.OrcamentoGerado, chave, cancellationToken);
    }
}

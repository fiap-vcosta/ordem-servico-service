using Application.Abstractions.Gateways;
using Domain.Entities;

namespace Application.Messaging;

public static class IntegrationEventIdempotency
{
    public static async Task<bool> AlreadyProcessedAsync(
        IEventoProcessadoGateway gateway,
        Guid eventId,
        string tipo,
        string chaveNegocio,
        CancellationToken cancellationToken)
    {
        if (await gateway.ExisteEventIdAsync(eventId, cancellationToken)
            || await gateway.ExisteChaveNegocioAsync(tipo, chaveNegocio, cancellationToken))
        {
            return true;
        }

        return false;
    }

    public static Task MarkProcessedAsync(
        IEventoProcessadoGateway gateway,
        Guid eventId,
        string tipo,
        string chaveNegocio,
        CancellationToken cancellationToken) =>
        gateway.AddAsync(
            new EventoProcessado
            {
                EventId = eventId,
                Tipo = tipo,
                ChaveNegocio = chaveNegocio,
                ProcessadoEm = DateTimeOffset.UtcNow
            },
            cancellationToken);
}

using Application.Abstractions.Gateways;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Gateways;

public class EventoProcessadoGateway(AppDbContext context) : IEventoProcessadoGateway
{
    public Task<bool> ExisteEventIdAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        context.EventosProcessados.AnyAsync(e => e.EventId == eventId, cancellationToken);

    public Task<bool> ExisteChaveNegocioAsync(
        string tipo,
        string chaveNegocio,
        CancellationToken cancellationToken = default) =>
        context.EventosProcessados.AnyAsync(
            e => e.Tipo == tipo && e.ChaveNegocio == chaveNegocio,
            cancellationToken);

    public async Task AddAsync(EventoProcessado evento, CancellationToken cancellationToken = default)
    {
        await context.EventosProcessados.AddAsync(evento, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}

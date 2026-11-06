using Application.Abstractions.Messaging;
using Domain.OrdemServico.Entities;

namespace Infrastructure.Messaging;

public sealed class NullDiagnosticoFinalizadoPublisher : IDiagnosticoFinalizadoPublisher
{
    public Task PublishAsync(OrdemServicoAggregateRoot ordemServico, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

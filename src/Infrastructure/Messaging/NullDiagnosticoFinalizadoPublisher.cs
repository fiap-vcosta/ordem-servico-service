using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Messaging;
using Domain.OrdemServico.Entities;

namespace Infrastructure.Messaging;

[ExcludeFromCodeCoverage(Justification = "No-op until Pub/Sub publisher is wired.")]
public sealed class NullDiagnosticoFinalizadoPublisher : IDiagnosticoFinalizadoPublisher
{
    public Task PublishAsync(OrdemServicoAggregateRoot ordemServico, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

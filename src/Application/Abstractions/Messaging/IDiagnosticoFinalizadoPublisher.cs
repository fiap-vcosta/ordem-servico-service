using Domain.Entities;

namespace Application.Abstractions.Messaging;

public interface IDiagnosticoFinalizadoPublisher
{
    Task PublishAsync(OrdemServicoAggregateRoot ordemServico, CancellationToken cancellationToken);
}

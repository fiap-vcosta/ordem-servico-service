using Domain.Entities;

namespace Application.Abstractions.Messaging;

public interface IOrdemServicoIntegrationPublisher
{
    Task PublishDiagnosticoFinalizadoAsync(
        OrdemServicoAggregateRoot ordemServico,
        CancellationToken cancellationToken = default);

    Task PublishOrdemServicoAprovadaAsync(
        OrdemServicoAggregateRoot ordemServico,
        CancellationToken cancellationToken = default);

    Task PublishOrdemServicoFinalizadaAsync(
        OrdemServicoAggregateRoot ordemServico,
        CancellationToken cancellationToken = default);

    Task PublishOrdemServicoDescartadaAsync(
        OrdemServicoAggregateRoot ordemServico,
        CancellationToken cancellationToken = default);
}

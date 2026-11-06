using Application.Abstractions.Events;
using Application.UseCases.Commands.EnviarOrdemServicoParaDiagnostico;
using Domain.Events;
using MediatR;

namespace Application.UseCases.Policies;

public class EnviarOrdemServicoParaDiagnosticoPolicy(IMediator mediator)
    : INotificationHandler<DomainEventNotification<OrdemServicoCriadaEvent>>
{
    public async Task Handle(DomainEventNotification<OrdemServicoCriadaEvent> notification, CancellationToken cancellationToken)
    {
        await mediator.Send(new EnviarOrdemServicoParaDiagnosticoCommand
        {
            IdOrdemServico = notification.DomainEvent.IdOrdemServico
        }, cancellationToken);
    }
}

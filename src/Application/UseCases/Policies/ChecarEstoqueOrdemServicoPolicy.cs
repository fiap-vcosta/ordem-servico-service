using Application.Abstractions.Events;
using Application.UseCases.Commands.AlocarEstoqueOrdemServico;
using Domain.Events;
using MediatR;

namespace Application.UseCases.Policies;

public class ChecarEstoqueOrdemServicoPolicy(IMediator mediator) : INotificationHandler<DomainEventNotification<OrdemServicoAprovadaEvent>>
{
    public async Task Handle(DomainEventNotification<OrdemServicoAprovadaEvent> notification, CancellationToken cancellationToken)
    {
        await mediator.Send(new AlocarEstoqueOrdemServicoCommand(notification.DomainEvent.IdOrdemServico), cancellationToken);
    }
}

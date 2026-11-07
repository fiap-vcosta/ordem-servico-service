using Application.Abstractions.Events;
using Application.UseCases.Commands.AlocarEstoqueOrdemServico;
using Domain.Estoque.Events;
using Application.Abstractions.Gateways;
using MediatR;

namespace Application.UseCases.Policies;

public class ChecarFilaDeEsperaOrdemServicoPolicy(
    IOrdemServicoGateway ordemServicoGateway,
    IMediator mediator
) : INotificationHandler<DomainEventNotification<ChegadaDeItensRegistradaEvent>>
{
    public async Task Handle(DomainEventNotification<ChegadaDeItensRegistradaEvent> notification, CancellationToken cancellationToken)
    {
        var idItemAtualizado = notification.DomainEvent.ItemEstoqueAggregateRoot.Id;
        
        var filaOrdemServico = await ordemServicoGateway.GetAguardandoPecaPorItemEstoqueAsync(idItemAtualizado);

        foreach (var ordemServico in filaOrdemServico)
        {
            await mediator.Send(new AlocarEstoqueOrdemServicoCommand(ordemServico.Id), cancellationToken);
        }
    }
}

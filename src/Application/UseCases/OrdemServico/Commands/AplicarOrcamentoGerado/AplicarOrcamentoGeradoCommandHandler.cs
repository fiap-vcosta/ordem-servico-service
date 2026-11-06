using Application.Abstractions.Events;
using Application.Abstractions.Gateways;
using Application.Logging;
using Domain.Exceptions;
using Domain.OrdemServico.Entities;
using Domain.OrdemServico.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.OrdemServico.Commands.AplicarOrcamentoGerado;

public class AplicarOrcamentoGeradoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    IMediator mediator,
    ILogger<AplicarOrcamentoGeradoCommandHandler> logger
) : IRequestHandler<AplicarOrcamentoGeradoCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(AplicarOrcamentoGeradoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        if (ordemServico.Status is not StatusOrdemServico.AguardandoOrcamento
            || ordemServico.VersaoDiagnostico != request.VersaoDiagnostico)
        {
            return OrdemServicoResponse.From(ordemServico);
        }

        ordemServico.AplicarOrcamentoGerado(request.VersaoDiagnostico);
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        if (ordemServico.Status is StatusOrdemServico.ChecandoEstoque)
        {
            await mediator.Publish(
                new DomainEventNotification<OrdemServicoAprovadaEvent>(new OrdemServicoAprovadaEvent(ordemServico.Id)),
                cancellationToken);
        }

        return OrdemServicoResponse.From(ordemServico);
    }
}

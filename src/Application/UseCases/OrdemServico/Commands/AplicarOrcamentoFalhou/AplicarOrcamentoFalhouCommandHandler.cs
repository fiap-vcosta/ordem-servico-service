using Application.Abstractions.Gateways;
using Application.Logging;
using Domain.Exceptions;
using Domain.OrdemServico.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.OrdemServico.Commands.AplicarOrcamentoFalhou;

public class AplicarOrcamentoFalhouCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    ILogger<AplicarOrcamentoFalhouCommandHandler> logger
) : IRequestHandler<AplicarOrcamentoFalhouCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(AplicarOrcamentoFalhouCommand request, CancellationToken cancellationToken)
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

        ordemServico.AplicarOrcamentoFalhou(request.VersaoDiagnostico);
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        return OrdemServicoResponse.From(ordemServico);
    }
}

using Application.Abstractions.Gateways;
using Application.Abstractions.Messaging;
using Application.Logging;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands.AplicarOrcamentoGerado;

public class AplicarOrcamentoGeradoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    IOrdemServicoIntegrationPublisher integrationPublisher,
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
            await integrationPublisher.PublishOrdemServicoAprovadaAsync(ordemServico, cancellationToken);
        }

        return OrdemServicoResponse.From(ordemServico);
    }
}

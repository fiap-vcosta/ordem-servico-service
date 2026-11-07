using Application.Abstractions.Gateways;
using Application.Logging;
using Domain.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands.AplicarCobrancaGerada;

public class AplicarCobrancaGeradaCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    ILogger<AplicarCobrancaGeradaCommandHandler> logger
) : IRequestHandler<AplicarCobrancaGeradaCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(AplicarCobrancaGeradaCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        if (ordemServico.Status is not (StatusOrdemServico.Finalizada or StatusOrdemServico.AguardandoPagamento))
        {
            return OrdemServicoResponse.From(ordemServico);
        }

        ordemServico.RegistrarCobrancaGerada();
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        return OrdemServicoResponse.From(ordemServico);
    }
}

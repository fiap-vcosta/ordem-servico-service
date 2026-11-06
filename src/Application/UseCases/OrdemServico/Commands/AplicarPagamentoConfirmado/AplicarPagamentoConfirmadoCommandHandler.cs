using Application.Abstractions.Gateways;
using Application.Logging;
using Domain.Exceptions;
using Domain.OrdemServico.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.OrdemServico.Commands.AplicarPagamentoConfirmado;

public class AplicarPagamentoConfirmadoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    ILogger<AplicarPagamentoConfirmadoCommandHandler> logger
) : IRequestHandler<AplicarPagamentoConfirmadoCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(AplicarPagamentoConfirmadoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        if (ordemServico.Status is StatusOrdemServico.Entregue)
        {
            return OrdemServicoResponse.From(ordemServico);
        }

        if (ordemServico.Status is not StatusOrdemServico.AguardandoPagamento)
        {
            return OrdemServicoResponse.From(ordemServico);
        }

        ordemServico.ConfirmarPagamentoRecebido();
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        return OrdemServicoResponse.From(ordemServico);
    }
}

using Application.Abstractions.Gateways;
using Application.Logging;
using Domain.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands.AplicarPagamentoRecusado;

public class AplicarPagamentoRecusadoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    ILogger<AplicarPagamentoRecusadoCommandHandler> logger
) : IRequestHandler<AplicarPagamentoRecusadoCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(AplicarPagamentoRecusadoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        if (ordemServico.Status is StatusOrdemServico.CobrancaExpirada
            && string.Equals(request.Motivo, OrdemServicoAggregateRoot.MotivoPagamentoExpirado, StringComparison.Ordinal))
        {
            return OrdemServicoResponse.From(ordemServico);
        }

        if (ordemServico.Status is not StatusOrdemServico.AguardandoPagamento)
        {
            return OrdemServicoResponse.From(ordemServico);
        }

        var statusAnterior = ordemServico.Status;
        ordemServico.RegistrarPagamentoRecusado(request.Motivo);
        await ordemServicoGateway.UpdateAsync(ordemServico);

        if (ordemServico.Status != statusAnterior)
        {
            logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);
        }

        return OrdemServicoResponse.From(ordemServico);
    }
}

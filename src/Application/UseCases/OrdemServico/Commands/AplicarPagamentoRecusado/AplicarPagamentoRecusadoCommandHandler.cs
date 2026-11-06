using Application.Abstractions.Gateways;
using Domain.Exceptions;
using Domain.OrdemServico.Entities;
using MediatR;

namespace Application.UseCases.OrdemServico.Commands.AplicarPagamentoRecusado;

public class AplicarPagamentoRecusadoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway
) : IRequestHandler<AplicarPagamentoRecusadoCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(AplicarPagamentoRecusadoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        if (ordemServico.Status is not StatusOrdemServico.AguardandoPagamento)
        {
            return OrdemServicoResponse.From(ordemServico);
        }

        ordemServico.RegistrarPagamentoRecusado();
        return OrdemServicoResponse.From(ordemServico);
    }
}

using Application.Abstractions.Gateways;
using Application.Logging;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands.AplicarEstoqueReservado;

public class AplicarEstoqueReservadoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    ILogger<AplicarEstoqueReservadoCommandHandler> logger
) : IRequestHandler<AplicarEstoqueReservadoCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(
        AplicarEstoqueReservadoCommand request,
        CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        if (ordemServico.Status is not (StatusOrdemServico.ChecandoEstoque or StatusOrdemServico.AguardandoPeca)
            || ordemServico.AprovadaEm is null
            || new DateTimeOffset(DateTime.SpecifyKind(ordemServico.AprovadaEm.Value, DateTimeKind.Utc)) != request.AprovadaEm)
        {
            return OrdemServicoResponse.From(ordemServico);
        }

        var statusPorItem = request.Itens.ToDictionary(i => i.ItemEstoqueId, i => i.Status);
        ordemServico.AplicarEstoqueReservado(statusPorItem);
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        return OrdemServicoResponse.From(ordemServico);
    }
}

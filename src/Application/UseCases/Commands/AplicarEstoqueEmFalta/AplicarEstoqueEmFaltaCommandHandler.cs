using Application.Abstractions.Gateways;
using Application.Logging;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands.AplicarEstoqueEmFalta;

public class AplicarEstoqueEmFaltaCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    ILogger<AplicarEstoqueEmFaltaCommandHandler> logger
) : IRequestHandler<AplicarEstoqueEmFaltaCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(
        AplicarEstoqueEmFaltaCommand request,
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
        ordemServico.AplicarEstoqueEmFalta(statusPorItem);
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        return OrdemServicoResponse.From(ordemServico);
    }
}

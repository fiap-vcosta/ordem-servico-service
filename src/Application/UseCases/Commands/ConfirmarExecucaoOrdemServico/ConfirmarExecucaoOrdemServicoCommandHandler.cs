using System.Transactions;
using Application.Abstractions.Gateways;
using Application.UseCases.Estoque.ItemEstoque.Commands.ConfirmarUtilizacaoItensEstoque;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Logging;

namespace Application.UseCases.Commands.ConfirmarExecucaoOrdemServico;

public class ConfirmarExecucaoOrdemServicoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    IMediator mediator,
    ILogger<ConfirmarExecucaoOrdemServicoCommandHandler> logger
) : IRequestHandler<ConfirmarExecucaoOrdemServicoCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(ConfirmarExecucaoOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        foreach (var servico in request.ServicosExecutados.Where(servico => ordemServico.Servicos.All(s => s.Id != servico.IdServico)))
        {
            throw new DomainNotFoundException($"Serviço com id {servico.IdServico} não pertence à Ordem de Serviço {request.IdOrdemServico}");
        }

        var opcoesTransacao = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
        using var scope = new TransactionScope(TransactionScopeOption.Required, opcoesTransacao, TransactionScopeAsyncFlowOption.Enabled);

        var command = new ConfirmarUtilizacaoItensEstoqueCommand { IdOrdemServico = ordemServico.Id };
        await mediator.Send(command, cancellationToken);

        ordemServico.ConfirmarExecucao(request.ServicosExecutados);
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        scope.Complete();
        return OrdemServicoResponse.From(ordemServico);
    }
}

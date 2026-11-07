using System.Transactions;
using Application.Abstractions.Gateways;
using Application.UseCases.Estoque.ItemEstoque.Commands.EnviarNotificacaoParaCompra;
using Application.UseCases.Estoque.ItemEstoque.Commands.TravarItensNecessarios;
using Domain.Exceptions;
using Domain.Estoque.Entities;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Logging;

namespace Application.UseCases.Commands.AlocarEstoqueOrdemServico;

public class AlocarEstoqueOrdemServicoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    IItemEstoqueGateway itemEstoqueGateway,
    IMediator mediator,
    ILogger<AlocarEstoqueOrdemServicoCommandHandler> logger
) : IRequestHandler<AlocarEstoqueOrdemServicoCommand, Unit>
{
    private async Task TravarEstoqueParaOrdemServico(OrdemServicoAggregateRoot ordemServico, IEnumerable<ItemEstoqueAggregateRoot> itensEstoque)
    {
        foreach (var item in itensEstoque)
        {
            var saldoNecessario = ordemServico.ItensNecessariosParaExecucao
                .Where(i => i.ItemEstoque.Id == item.Id)
                .Sum(i => i.Quantidade);

            var command = new TravarItensNecessariosCommand
            {
                IdItemEstoque = item.Id,
                QuantidadeNecessaria = saldoNecessario
            };

            await mediator.Send(command);
        }
        
        ordemServico.TravarItensNecessarios();
    }

    private async Task SolicitarPecasEmFaltaOrdemServico(OrdemServicoAggregateRoot ordemServico, Dictionary<int, decimal> saldosDisponiveis)
    {
        var itensNecessariosAgrupados = ordemServico.ItensNecessariosParaExecucao
            .GroupBy(i => i.ItemEstoque.Id)
            .Select(grupo => new 
            {
                IdItemEstoque = grupo.Key,
                NomeItem = grupo.First().ItemEstoque.Nome,
                QuantidadeTotalNecessaria = grupo.Sum(i => i.Quantidade)
            });
        
        foreach (var item in itensNecessariosAgrupados)
        {
            var saldoDisponivel = saldosDisponiveis[item.IdItemEstoque];
            var quantidadeFaltando = item.QuantidadeTotalNecessaria - saldoDisponivel;

            if (quantidadeFaltando > 0)
            {
                var command = new EnviarNotificacaoParaCompraCommand
                {
                    IdItemEstoque = item.IdItemEstoque,
                    IdOrdemServico = ordemServico.Id,
                    NomeItem = item.NomeItem,
                    QuantidadeFaltando = quantidadeFaltando
                };

                await mediator.Send(command);   
            }
        }
    }
    
    public async Task<Unit> Handle(AlocarEstoqueOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        var opcoesTransacao = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
        using var scope = new TransactionScope(TransactionScopeOption.Required, opcoesTransacao, TransactionScopeAsyncFlowOption.Enabled);
        
        var idsItensEstoque = ordemServico.Servicos
            .SelectMany(s => s.ItensNecessarios)
            .Select(i => i.ItemEstoque.Id)
            .Distinct()
            .ToList();
        
        var itensEstoque = (await itemEstoqueGateway.GetEBloquearItensAsync(idsItensEstoque)).ToList();
        var saldosDisponiveis = itensEstoque.ToDictionary(item => item.Id, item => item.SaldoDisponivel);

        ordemServico.ChecarItensNecessarios(saldosDisponiveis);

        switch (ordemServico.Status)
        {
            case StatusOrdemServico.LiberadaParaExecucao:
                await TravarEstoqueParaOrdemServico(ordemServico, itensEstoque);
                break;
            case StatusOrdemServico.AguardandoPeca:
                await SolicitarPecasEmFaltaOrdemServico(ordemServico, saldosDisponiveis);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Status {ordemServico.Status} inválido após checar estoque para ordem de serviço {ordemServico.Id}.");
        }
        
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);
        scope.Complete();

        return Unit.Value;
    }
}

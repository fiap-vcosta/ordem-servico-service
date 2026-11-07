using Application.Abstractions.Gateways;
using Application.UseCases.Responses;
using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;
using MediatR;

namespace Application.UseCases.Commands.AdicionarItemOrdemServico;

public class AdicionarItemOrdemServicoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    IAdministrativoCatalogGateway administrativoCatalogGateway,
    IItemEstoqueGateway itemEstoqueGateway
) : IRequestHandler<AdicionarItemOrdemServicoCommand, AdicionarItemOrdemServicoCommandResponse>
{
    public async Task<AdicionarItemOrdemServicoCommandResponse> Handle(AdicionarItemOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        var servico = await administrativoCatalogGateway.GetServicoByIdAsync(request.IdServico, cancellationToken);
        if (servico == null)
        {
            throw new DomainNotFoundException($"Serviço com id {request.IdServico} não encontrado");
        }

        var itensNecessarios = new List<ItemNecessario.CriarItemNecessarioParams>();
        foreach (var itemNecessario in request.ItensNecessarios)
        {
            var itemEstoque = await itemEstoqueGateway.GetByIdAsync(itemNecessario.IdItemEstoque);
            if (itemEstoque == null)
            {
                throw new DomainNotFoundException($"Item de estoque com id {itemNecessario.IdItemEstoque} não encontrado");
            }

            var itemEstoqueOrdemServico = new ItemEstoqueOrdemServico
            {
                Id = itemEstoque.Id,
                Codigo = itemEstoque.Codigo,
                Nome = itemEstoque.Nome,
                UnidadeMedida = itemEstoque.UnidadeMedida.ToString()
            };

            itensNecessarios.Add(new ItemNecessario.CriarItemNecessarioParams(ordemServico.Id, itemNecessario.Quantidade, itemEstoqueOrdemServico));
        }

        var servicoCatalogo = new ServicoCatalogo
        {
            Id = servico.Id,
            Nome = servico.Nome,
            Codigo = servico.Codigo
        };

        ordemServico.AdicionarItemServico(servico.Nome, request.ValorCobrado, servicoCatalogo, itensNecessarios);
        await ordemServicoGateway.UpdateAsync(ordemServico);

        return new AdicionarItemOrdemServicoCommandResponse
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            ValorTotal = ordemServico.ValorTotal,
            RecebidaEm = ordemServico.RecebidaEm,
            Cliente = ClienteOrdemServicoResponse.From(ordemServico.Cliente),
            Veiculo = VeiculoOrdemServicoResponse.From(ordemServico.Veiculo),
            Itens = ServicoOrdemServicoResponse.FromMany(ordemServico.Servicos)
        };
    }
}

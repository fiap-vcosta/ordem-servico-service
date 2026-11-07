using Application.UseCases.Responses;
using Domain.Entities;

namespace Api.ViewModels.OrdemServico;

public record ItemNecessarioViewModel
{
    public required int Id { get; init; }
    public required int IdOrdemServico { get; init; }
    public required int IdItemOrdemServico { get; init; }
    public required StatusItemEstoque Status { get; init; }
    public required decimal Quantidade { get; init; }
    public required ItemEstoqueOrdemServicoViewModel ItemEstoque { get; init; }

    public static ItemNecessarioViewModel From(ItemNecessarioResponse response)
    {
        return new ItemNecessarioViewModel
        {
            Id = response.Id,
            IdOrdemServico = response.IdOrdemServico,
            IdItemOrdemServico = response.IdItemOrdemServico,
            Status = response.Status,
            Quantidade = response.Quantidade,
            ItemEstoque = ItemEstoqueOrdemServicoViewModel.From(response.ItemEstoque)
        };
    }

    public static List<ItemNecessarioViewModel> FromMany(IEnumerable<ItemNecessarioResponse> responses)
    {
        return responses.Select(From).ToList();
    }
}

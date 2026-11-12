using Domain.Entities;

namespace Application.UseCases.Responses;

public class ItemNecessarioResponse
{
    public int Id { get; init; }
    public Guid IdOrdemServico { get; init; }
    public int IdItemOrdemServico { get; init; }
    public StatusItemEstoque Status { get; init; }
    public decimal Quantidade { get; init; }
    public required ItemEstoqueOrdemServicoResponse ItemEstoque { get; init; }

    public static ItemNecessarioResponse From(ItemNecessario itemNecessario)
    {
        return new ItemNecessarioResponse
        {
            Id = itemNecessario.Id,
            IdOrdemServico = itemNecessario.IdOrdemServico,
            IdItemOrdemServico = itemNecessario.IdItemOrdemServico,
            Status = itemNecessario.Status,
            Quantidade = itemNecessario.Quantidade,
            ItemEstoque = ItemEstoqueOrdemServicoResponse.From(itemNecessario.ItemEstoque)
        };
    }

    public static List<ItemNecessarioResponse> FromMany(IEnumerable<ItemNecessario> itens)
    {
        return itens.Select(From).ToList();
    }
}

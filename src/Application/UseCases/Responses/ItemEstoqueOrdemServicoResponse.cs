using Domain.ValueObjects;

namespace Application.UseCases.Responses;

public class ItemEstoqueOrdemServicoResponse
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string UnidadeMedida { get; init; } = string.Empty;

    public static ItemEstoqueOrdemServicoResponse From(ItemEstoqueOrdemServico itemEstoque)
    {
        return new ItemEstoqueOrdemServicoResponse
        {
            Id = itemEstoque.Id,
            Codigo = itemEstoque.Codigo,
            Nome = itemEstoque.Nome,
            UnidadeMedida = itemEstoque.UnidadeMedida
        };
    }
}

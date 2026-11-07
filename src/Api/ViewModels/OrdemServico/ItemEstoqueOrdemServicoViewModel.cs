using Application.UseCases.Responses;

namespace Api.ViewModels.OrdemServico;

public record ItemEstoqueOrdemServicoViewModel
{
    public required int Id { get; init; }
    public required string Codigo { get; init; }
    public required string Nome { get; init; }
    public required string UnidadeMedida { get; init; }

    public static ItemEstoqueOrdemServicoViewModel From(ItemEstoqueOrdemServicoResponse response)
    {
        return new ItemEstoqueOrdemServicoViewModel
        {
            Id = response.Id,
            Codigo = response.Codigo,
            Nome = response.Nome,
            UnidadeMedida = response.UnidadeMedida
        };
    }
}

using Application.UseCases.Responses;

namespace Api.ViewModels.OrdemServico;

public record ServicoCatalogoViewModel
{
    public required int Id { get; init; }
    public required string Nome { get; init; }
    public required string Codigo { get; init; }

    public static ServicoCatalogoViewModel From(ServicoCatalogoResponse response)
    {
        return new ServicoCatalogoViewModel
        {
            Id = response.Id,
            Nome = response.Nome,
            Codigo = response.Codigo
        };
    }
}

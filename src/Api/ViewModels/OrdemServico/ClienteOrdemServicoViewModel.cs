using Application.UseCases.Responses;

namespace Api.ViewModels.OrdemServico;

public record ClienteOrdemServicoViewModel
{
    public required int Id { get; init; }
    public required string Nome { get; init; }
    public required string Email { get; init; }

    public static ClienteOrdemServicoViewModel From(ClienteOrdemServicoResponse response)
    {
        return new ClienteOrdemServicoViewModel
        {
            Id = response.Id,
            Nome = response.Nome,
            Email = response.Email
        };
    }
}

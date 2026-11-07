using MediatR;

namespace Application.UseCases.Commands.AplicarCobrancaGerada;

public class AplicarCobrancaGeradaCommand : IRequest<OrdemServicoResponse>
{
    public required int IdOrdemServico { get; init; }
}

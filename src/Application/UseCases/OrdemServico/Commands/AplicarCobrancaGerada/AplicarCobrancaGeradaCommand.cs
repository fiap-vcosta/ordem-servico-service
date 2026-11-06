using MediatR;

namespace Application.UseCases.OrdemServico.Commands.AplicarCobrancaGerada;

public class AplicarCobrancaGeradaCommand : IRequest<OrdemServicoResponse>
{
    public required int IdOrdemServico { get; init; }
}

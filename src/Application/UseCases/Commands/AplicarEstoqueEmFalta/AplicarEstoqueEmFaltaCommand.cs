using Application.UseCases.Commands.AplicarEstoqueReservado;
using MediatR;

namespace Application.UseCases.Commands.AplicarEstoqueEmFalta;

public class AplicarEstoqueEmFaltaCommand : IRequest<OrdemServicoResponse>
{
    public required int IdOrdemServico { get; init; }

    public required DateTimeOffset AprovadaEm { get; init; }

    public required IReadOnlyList<AplicarEstoqueItemStatus> Itens { get; init; }
}

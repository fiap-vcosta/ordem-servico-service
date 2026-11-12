using Application.UseCases.Commands.AplicarEstoqueReservado;
using MediatR;

namespace Application.UseCases.Commands.AplicarEstoqueEmFalta;

public class AplicarEstoqueEmFaltaCommand : IRequest<OrdemServicoResponse>
{
    public required Guid IdOrdemServico { get; init; }

    public required DateTimeOffset AprovadaEm { get; init; }

    public required IReadOnlyList<AplicarEstoqueItemStatus> Itens { get; init; }
}

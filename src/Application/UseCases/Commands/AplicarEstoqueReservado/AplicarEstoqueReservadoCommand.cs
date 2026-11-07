using MediatR;

namespace Application.UseCases.Commands.AplicarEstoqueReservado;

public class AplicarEstoqueReservadoCommand : IRequest<OrdemServicoResponse>
{
    public required int IdOrdemServico { get; init; }

    public required DateTimeOffset AprovadaEm { get; init; }

    public required IReadOnlyList<AplicarEstoqueItemStatus> Itens { get; init; }
}

public sealed record AplicarEstoqueItemStatus(int ItemEstoqueId, Domain.Entities.StatusItemEstoque Status);

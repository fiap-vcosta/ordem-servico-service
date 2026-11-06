using MediatR;

namespace Application.UseCases.OrdemServico.Commands.AplicarPagamentoConfirmado;

public class AplicarPagamentoConfirmadoCommand : IRequest<OrdemServicoResponse>
{
    public required int IdOrdemServico { get; init; }
}

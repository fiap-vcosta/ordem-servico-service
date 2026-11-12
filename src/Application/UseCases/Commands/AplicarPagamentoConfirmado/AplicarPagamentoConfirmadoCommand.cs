using MediatR;

namespace Application.UseCases.Commands.AplicarPagamentoConfirmado;

public class AplicarPagamentoConfirmadoCommand : IRequest<OrdemServicoResponse>
{
    public required Guid IdOrdemServico { get; init; }
}

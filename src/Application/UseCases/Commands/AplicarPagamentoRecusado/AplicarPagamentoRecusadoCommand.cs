using MediatR;

namespace Application.UseCases.Commands.AplicarPagamentoRecusado;

public class AplicarPagamentoRecusadoCommand : IRequest<OrdemServicoResponse>
{
    public required int IdOrdemServico { get; init; }
}

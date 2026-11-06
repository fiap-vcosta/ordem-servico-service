using MediatR;

namespace Application.UseCases.OrdemServico.Commands.AplicarPagamentoRecusado;

public class AplicarPagamentoRecusadoCommand : IRequest<OrdemServicoResponse>
{
    public required int IdOrdemServico { get; init; }
}

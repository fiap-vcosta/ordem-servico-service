using MediatR;

namespace Application.UseCases.Commands.AplicarPagamentoRecusado;

public class AplicarPagamentoRecusadoCommand : IRequest<OrdemServicoResponse>
{
    public required Guid IdOrdemServico { get; init; }

    public required string Motivo { get; init; }
}

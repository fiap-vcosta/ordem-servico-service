using MediatR;

namespace Application.UseCases.Commands.RejeitarOrdemServico;

public class RejeitarOrdemServicoCommand : IRequest<RejeitarOrdemServicoCommandResponse>
{
    public int IdOrdemServico { get; init; }
}
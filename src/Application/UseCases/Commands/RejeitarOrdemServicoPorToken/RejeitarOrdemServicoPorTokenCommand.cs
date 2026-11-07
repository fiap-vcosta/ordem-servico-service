using Application.UseCases.Commands.RejeitarOrdemServico;
using MediatR;

namespace Application.UseCases.Commands.RejeitarOrdemServicoPorToken;

public class RejeitarOrdemServicoPorTokenCommand : IRequest<RejeitarOrdemServicoCommandResponse>
{
    public required string TokenAprovacao { get; init; }
    public required string DocumentoCliente { get; init; }
}

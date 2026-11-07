using Application.UseCases.Commands.AprovarOrdemServico;
using MediatR;

namespace Application.UseCases.Commands.AprovarOrdemServicoPorToken;

public class AprovarOrdemServicoPorTokenCommand : IRequest<AprovarOrdemServicoCommandResponse>
{
    public required string TokenAprovacao { get; init; }
    public required string DocumentoCliente { get; init; }
}

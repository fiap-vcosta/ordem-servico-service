using MediatR;

namespace Application.UseCases.Commands.AprovarOrdemServico;

public class AprovarOrdemServicoCommand : IRequest<AprovarOrdemServicoCommandResponse>
{
    public int IdOrdemServico { get; init; }
}
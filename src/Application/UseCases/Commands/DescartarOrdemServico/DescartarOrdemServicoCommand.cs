using MediatR;

namespace Application.UseCases.Commands.DescartarOrdemServico;

public class DescartarOrdemServicoCommand : IRequest<DescartarOrdemServicoResponse>
{
    public int IdOrdemServico { get; init; }
}
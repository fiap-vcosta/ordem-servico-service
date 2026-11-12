using MediatR;

namespace Application.UseCases.Commands.DescartarOrdemServico;

public class DescartarOrdemServicoCommand : IRequest<DescartarOrdemServicoResponse>
{
    public Guid IdOrdemServico { get; init; }
}
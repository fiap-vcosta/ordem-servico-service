using MediatR;

namespace Application.UseCases.Commands.AprovarServicosParcialmente;

public class AprovarServicosParcialmenteCommand : IRequest<AprovarServicosParcialmenteCommandResponse>
{
    public int IdOrdemServico { get; init; }
    public List<int> IdServicosAprovados { get; init; } = [];
}
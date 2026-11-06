using MediatR;

namespace Application.UseCases.Queries.GetOrdemServicoById;

public class GetOrdemServicoByIdQuery : IRequest<OrdemServicoResponse?>
{
    public int Id { get; init; }
}

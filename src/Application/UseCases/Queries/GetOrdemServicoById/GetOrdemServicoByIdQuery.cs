using MediatR;

namespace Application.UseCases.Queries.GetOrdemServicoById;

public class GetOrdemServicoByIdQuery : IRequest<OrdemServicoResponse?>
{
    public Guid Id { get; init; }
}

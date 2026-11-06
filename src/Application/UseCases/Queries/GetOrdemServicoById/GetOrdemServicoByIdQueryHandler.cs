using Application.Abstractions.Gateways;
using MediatR;

namespace Application.UseCases.Queries.GetOrdemServicoById;

public class GetOrdemServicoByIdQueryHandler(IOrdemServicoGateway ordemServicoGateway) : IRequestHandler<GetOrdemServicoByIdQuery, OrdemServicoResponse?>
{
    public async Task<OrdemServicoResponse?> Handle(GetOrdemServicoByIdQuery request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.Id);
        return ordemServico == null ? null : OrdemServicoResponse.From(ordemServico);
    }
}

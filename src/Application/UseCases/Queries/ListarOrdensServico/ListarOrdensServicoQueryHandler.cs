using Application.Abstractions.Gateways;
using MediatR;

namespace Application.UseCases.Queries.ListarOrdensServico;

public class ListarOrdensServicoQueryHandler(IOrdemServicoGateway ordemServicoGateway)
    : IRequestHandler<ListarOrdensServicoQuery, IReadOnlyList<OrdemServicoResponse>>
{
    public async Task<IReadOnlyList<OrdemServicoResponse>> Handle(ListarOrdensServicoQuery request, CancellationToken cancellationToken)
    {
        var ordens = await ordemServicoGateway.ListarAtivasAsync();
        return ordens.Select(OrdemServicoResponse.From).ToList();
    }
}

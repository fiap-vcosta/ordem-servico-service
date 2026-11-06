using Application.Abstractions.Gateways;
using Application.UseCases.Responses;
using MediatR;

namespace Application.UseCases.Queries.GetTempoMedioAllServicos;

public class GetTempoMedioExecucaoAllServicosQueryHandler(IItemServicoGateway itemServicoGateway)
    : IRequestHandler<GetTempoMedioExecucaoAllServicosQuery, List<TempoMedioExecucaoResponse>>
{
    public async Task<List<TempoMedioExecucaoResponse>> Handle(GetTempoMedioExecucaoAllServicosQuery request, CancellationToken cancellationToken)
    {
        var temposMedios = await itemServicoGateway.GetAllTempoMedioExecucaoAsync();
        return temposMedios
            .Select(t => TempoMedioExecucaoResponse.From(t.IdServico, t.TotalExecucoes, t.ExecucaoMedia))
            .ToList();
    }
}
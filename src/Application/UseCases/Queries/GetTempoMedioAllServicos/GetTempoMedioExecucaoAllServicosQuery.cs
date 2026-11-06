using Application.UseCases.Responses;
using MediatR;

namespace Application.UseCases.Queries.GetTempoMedioAllServicos;

public class GetTempoMedioExecucaoAllServicosQuery : IRequest<List<TempoMedioExecucaoResponse>>;
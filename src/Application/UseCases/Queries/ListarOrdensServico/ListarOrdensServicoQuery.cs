using MediatR;

namespace Application.UseCases.Queries.ListarOrdensServico;

public class ListarOrdensServicoQuery : IRequest<IReadOnlyList<OrdemServicoResponse>>;

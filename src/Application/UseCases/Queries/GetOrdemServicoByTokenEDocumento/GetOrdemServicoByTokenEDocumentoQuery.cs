using MediatR;

namespace Application.UseCases.Queries.GetOrdemServicoByTokenEDocumento;

public class GetOrdemServicoByTokenEDocumentoQuery : IRequest<Guid>
{
    public required string TokenAprovacao { get; init; }
    public required string DocumentoCliente { get; init; }
}

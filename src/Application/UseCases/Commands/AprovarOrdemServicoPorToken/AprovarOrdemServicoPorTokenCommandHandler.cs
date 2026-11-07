using Application.UseCases.Commands.AprovarOrdemServico;
using Application.UseCases.Queries.GetOrdemServicoByTokenEDocumento;
using MediatR;

namespace Application.UseCases.Commands.AprovarOrdemServicoPorToken;

public class AprovarOrdemServicoPorTokenCommandHandler(IMediator mediator)
    : IRequestHandler<AprovarOrdemServicoPorTokenCommand, AprovarOrdemServicoCommandResponse>
{
    public async Task<AprovarOrdemServicoCommandResponse> Handle(
        AprovarOrdemServicoPorTokenCommand request,
        CancellationToken cancellationToken)
    {
        var idOrdemServico = await mediator.Send(
            new GetOrdemServicoByTokenEDocumentoQuery
            {
                TokenAprovacao = request.TokenAprovacao,
                DocumentoCliente = request.DocumentoCliente
            },
            cancellationToken);

        return await mediator.Send(
            new AprovarOrdemServicoCommand { IdOrdemServico = idOrdemServico },
            cancellationToken);
    }
}

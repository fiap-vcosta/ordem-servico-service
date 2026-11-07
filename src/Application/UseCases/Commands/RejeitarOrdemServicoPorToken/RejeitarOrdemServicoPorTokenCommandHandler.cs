using Application.UseCases.Commands.RejeitarOrdemServico;
using Application.UseCases.Queries.GetOrdemServicoByTokenEDocumento;
using MediatR;

namespace Application.UseCases.Commands.RejeitarOrdemServicoPorToken;

public class RejeitarOrdemServicoPorTokenCommandHandler(IMediator mediator)
    : IRequestHandler<RejeitarOrdemServicoPorTokenCommand, RejeitarOrdemServicoCommandResponse>
{
    public async Task<RejeitarOrdemServicoCommandResponse> Handle(
        RejeitarOrdemServicoPorTokenCommand request,
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
            new RejeitarOrdemServicoCommand { IdOrdemServico = idOrdemServico },
            cancellationToken);
    }
}

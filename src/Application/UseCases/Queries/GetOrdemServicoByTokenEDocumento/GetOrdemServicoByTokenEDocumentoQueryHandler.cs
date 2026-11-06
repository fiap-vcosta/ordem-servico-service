using Application.Abstractions.Gateways;
using Domain.Exceptions;
using MediatR;

namespace Application.UseCases.Queries.GetOrdemServicoByTokenEDocumento;

public class GetOrdemServicoByTokenEDocumentoQueryHandler(IOrdemServicoGateway ordemServicoGateway)
    : IRequestHandler<GetOrdemServicoByTokenEDocumentoQuery, int>
{
    private const string TokenNaoEncontradoMessage = "Ordem de Serviço não encontrada para o token informado";

    public async Task<int> Handle(
        GetOrdemServicoByTokenEDocumentoQuery request,
        CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByTokenEDocumentoAsync(
            request.TokenAprovacao,
            request.DocumentoCliente);

        if (ordemServico is null)
        {
            throw new DomainNotFoundException(TokenNaoEncontradoMessage);
        }

        return ordemServico.Id;
    }
}

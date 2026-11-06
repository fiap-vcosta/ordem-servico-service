using Application.Abstractions.Gateways;
using Application.UseCases.Responses;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Logging;

namespace Application.UseCases.Commands.RejeitarOrdemServico;

public class RejeitarOrdemServicoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    ILogger<RejeitarOrdemServicoCommandHandler> logger
) : IRequestHandler<RejeitarOrdemServicoCommand, RejeitarOrdemServicoCommandResponse>
{
    public async Task<RejeitarOrdemServicoCommandResponse> Handle(RejeitarOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        ordemServico.RejeitarServicosSugeridos();

        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        return new RejeitarOrdemServicoCommandResponse()
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            ValorTotal = ordemServico.ValorTotal,
            RecebidaEm = ordemServico.RecebidaEm,
            Cliente = ClienteOrdemServicoResponse.From(ordemServico.Cliente),
            Veiculo = VeiculoOrdemServicoResponse.From(ordemServico.Veiculo),
            Servicos = ServicoOrdemServicoResponse.FromMany(ordemServico.Servicos)
        };
    }
}

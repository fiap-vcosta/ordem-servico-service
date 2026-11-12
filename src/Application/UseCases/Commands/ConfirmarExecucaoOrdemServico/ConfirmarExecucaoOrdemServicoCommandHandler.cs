using Application.Abstractions.Gateways;
using Application.Abstractions.Messaging;
using Application.Logging;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands.ConfirmarExecucaoOrdemServico;

public class ConfirmarExecucaoOrdemServicoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    IOrdemServicoIntegrationPublisher integrationPublisher,
    ILogger<ConfirmarExecucaoOrdemServicoCommandHandler> logger
) : IRequestHandler<ConfirmarExecucaoOrdemServicoCommand, OrdemServicoResponse>
{
    public async Task<OrdemServicoResponse> Handle(ConfirmarExecucaoOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        var servicoInvalido = request.ServicosExecutados
            .FirstOrDefault(servico => ordemServico.Servicos.All(s => s.Id != servico.IdServico));
        if (servicoInvalido is not null)
        {
            throw new DomainNotFoundException(
                $"Serviço com id {servicoInvalido.IdServico} não pertence à Ordem de Serviço {request.IdOrdemServico}");
        }

        ordemServico.ConfirmarExecucao(request.ServicosExecutados);
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        if (ordemServico.Status is StatusOrdemServico.Finalizada)
        {
            await integrationPublisher.PublishOrdemServicoFinalizadaAsync(ordemServico, cancellationToken);
        }

        return OrdemServicoResponse.From(ordemServico);
    }
}

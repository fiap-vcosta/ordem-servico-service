using Application.Abstractions.Gateways;
using Application.Abstractions.Messaging;
using Application.Logging;
using Application.UseCases.Responses;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands.AprovarOrdemServico;

public class AprovarOrdemServicoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    IOrdemServicoIntegrationPublisher integrationPublisher,
    ILogger<AprovarOrdemServicoCommandHandler> logger
) : IRequestHandler<AprovarOrdemServicoCommand, AprovarOrdemServicoCommandResponse>
{
    public async Task<AprovarOrdemServicoCommandResponse> Handle(AprovarOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        ordemServico.AprovarServicosSugeridos();

        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        if (ordemServico.Status is StatusOrdemServico.ChecandoEstoque)
        {
            await integrationPublisher.PublishOrdemServicoAprovadaAsync(ordemServico, cancellationToken);
        }

        return new AprovarOrdemServicoCommandResponse()
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            ValorTotal = ordemServico.ValorTotal,
            RecebidaEm = ordemServico.RecebidaEm,
            AprovadaEm = ordemServico.AprovadaEm ?? throw new BusinessRuleException("Data de aprovação precisa estar preenchida ao aprovar ordem de serviço"),
            Cliente = ClienteOrdemServicoResponse.From(ordemServico.Cliente),
            Veiculo = VeiculoOrdemServicoResponse.From(ordemServico.Veiculo),
            Servicos = ServicoOrdemServicoResponse.FromMany(ordemServico.Servicos)
        };
    }
}

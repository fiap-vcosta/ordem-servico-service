using Application.Abstractions.Gateways;
using Application.Abstractions.Messaging;
using Application.Logging;
using Application.UseCases.Responses;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands.AprovarServicosParcialmente;

public class AprovarServicosParcialmenteCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    IOrdemServicoIntegrationPublisher integrationPublisher,
    ILogger<AprovarServicosParcialmenteCommandHandler> logger
): IRequestHandler<AprovarServicosParcialmenteCommand, AprovarServicosParcialmenteCommandResponse>
{
    public async Task<AprovarServicosParcialmenteCommandResponse> Handle(AprovarServicosParcialmenteCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        var idsInvalidos = request.IdServicosAprovados
            .Where(idServicoAprovado => ordemServico.Servicos.All(ios => ios.Id != idServicoAprovado))
            .ToList();

        if (idsInvalidos.Count > 0)
        {
            throw new DomainNotFoundException($"Serviços [{string.Join(", ", idsInvalidos)}] não pertencem a Ordem de Serviço {request.IdOrdemServico}");
        }

        ordemServico.AprovarServicosParcialmente(request.IdServicosAprovados);
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        if (ordemServico.Status is StatusOrdemServico.ChecandoEstoque)
        {
            await integrationPublisher.PublishOrdemServicoAprovadaAsync(ordemServico, cancellationToken);
        }

        return new AprovarServicosParcialmenteCommandResponse()
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

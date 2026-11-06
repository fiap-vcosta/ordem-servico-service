using Application.Abstractions.Gateways;
using Application.Abstractions.Messaging;
using Application.Logging;
using Application.UseCases.OrdemServico.Responses;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.OrdemServico.Commands.FinalizarDiagnostico;

public class FinalizarDiagnosticoCommandHandler(
    IOrdemServicoGateway ordemServicoGateway,
    IDiagnosticoFinalizadoPublisher diagnosticoFinalizadoPublisher,
    ILogger<FinalizarDiagnosticoCommandHandler> logger
) : IRequestHandler<FinalizarDiagnosticoCommand, FinalizarDiagnosticoCommandResponse>
{
    public async Task<FinalizarDiagnosticoCommandResponse> Handle(FinalizarDiagnosticoCommand request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoGateway.GetByIdAsync(request.IdOrdemServico);
        if (ordemServico == null)
        {
            throw new DomainNotFoundException($"Ordem de Serviço com id {request.IdOrdemServico} não encontrada");
        }

        ordemServico.FinalizarDiagnostico();
        await ordemServicoGateway.UpdateAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        await diagnosticoFinalizadoPublisher.PublishAsync(ordemServico, cancellationToken);

        return new FinalizarDiagnosticoCommandResponse()
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            RecebidaEm = ordemServico.RecebidaEm,
            Cliente = ClienteOrdemServicoResponse.From(ordemServico.Cliente),
            Veiculo = VeiculoOrdemServicoResponse.From(ordemServico.Veiculo),
            Servicos = ServicoOrdemServicoResponse.FromMany(ordemServico.Servicos)
        };
    }
}

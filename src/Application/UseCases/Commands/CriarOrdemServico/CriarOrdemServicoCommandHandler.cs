using Application.Abstractions.Events;
using Application.Abstractions.Gateways;
using Application.Logging;
using Application.UseCases.Commands.AdicionarItemOrdemServico;
using Application.UseCases.Responses;
using Domain;
using Domain.Entities;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Commands.CriarOrdemServico;

public class CriarOrdemServicoCommandHandler(
    IAdministrativoCatalogGateway administrativoCatalogGateway,
    IOrdemServicoGateway ordemServicoGateway,
    IMediator mediator,
    ILogger<CriarOrdemServicoCommandHandler> logger
) : IRequestHandler<CriarOrdemServicoCommand, CriarOrdemServicoCommandResponse>
{
    public async Task<CriarOrdemServicoCommandResponse> Handle(CriarOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var veiculo = await administrativoCatalogGateway.GetVeiculoByIdAsync(request.IdVeiculo, cancellationToken);
        if (veiculo is null)
        {
            throw new DomainNotFoundException($"Veículo com id {request.IdVeiculo} não encontrado");
        }

        var cliente = await administrativoCatalogGateway.GetClienteByIdAsync(veiculo.IdCliente, cancellationToken);
        if (cliente is null)
        {
            throw new DomainNotFoundException($"Cliente com id {veiculo.IdCliente} não encontrado");
        }

        var clienteOrdemServico = new ClienteOrdemServico
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email,
            Documento = DocumentoNormalizer.Normalize(cliente.Documento),
        };

        var veiculoOrdemServico = new VeiculoOrdemServico
        {
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
        };

        var ordemServico = OrdemServicoAggregateRoot.Criar(clienteOrdemServico, veiculoOrdemServico);
        await ordemServicoGateway.CriarAsync(ordemServico);
        logger.LogStatusMovido(ordemServico.Id, ordemServico.Status);

        foreach (var servicoRequest in request.Servicos)
        {
            await mediator.Send(new AdicionarItemOrdemServicoCommand
            {
                IdOrdemServico = ordemServico.Id,
                IdServico = servicoRequest.IdServico,
                ValorCobrado = servicoRequest.ValorCobrado,
                ItensNecessarios = servicoRequest.ItensNecessarios.Select(item =>
                    new AdicionarItemOrdemServicoCommand.ItemNecessario
                    {
                        IdItemEstoque = item.IdItemEstoque,
                        Quantidade = item.Quantidade
                    }).ToList()
            }, cancellationToken);
        }

        await mediator.Publish(new DomainEventNotification<OrdemServicoCriadaEvent>(new OrdemServicoCriadaEvent(ordemServico.Id)), cancellationToken);

        var ordemAtualizada = await ordemServicoGateway.GetByIdAsync(ordemServico.Id)
            ?? throw new DomainNotFoundException($"Ordem de Serviço com id {ordemServico.Id} não encontrada");

        return new CriarOrdemServicoCommandResponse
        {
            Id = ordemAtualizada.Id,
            Status = ordemAtualizada.Status,
            ValorTotal = ordemAtualizada.ValorTotal,
            RecebidaEm = ordemAtualizada.RecebidaEm,
            Cliente = ClienteOrdemServicoResponse.From(ordemAtualizada.Cliente),
            Veiculo = VeiculoOrdemServicoResponse.From(ordemAtualizada.Veiculo),
            Servicos = ServicoOrdemServicoResponse.FromMany(ordemAtualizada.Servicos),
        };
    }
}

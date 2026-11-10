using Application.Abstractions.Messaging;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Domain.Entities;

namespace Infrastructure.Messaging;

public sealed class OrdemServicoIntegrationPublisher(IEventPublisher eventPublisher)
    : IOrdemServicoIntegrationPublisher
{
    public Task PublishDiagnosticoFinalizadoAsync(
        OrdemServicoAggregateRoot ordemServico,
        CancellationToken cancellationToken = default)
    {
        var data = new DiagnosticoFinalizadoData
        {
            OrdemServicoId = ordemServico.Id,
            VersaoDiagnostico = ordemServico.VersaoDiagnostico,
            Cliente = new DiagnosticoFinalizadoCliente
            {
                Nome = ordemServico.Cliente.Nome,
                Email = ordemServico.Cliente.Email,
                Documento = ordemServico.Cliente.Documento
            },
            Servicos = ordemServico.Servicos
                .Select(s => new DiagnosticoFinalizadoServico
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    ValorCobrado = s.ValorCobrado,
                    Status = MapServicoStatus(s.Status)
                })
                .ToList()
        };

        return eventPublisher.PublishAsync(
            TopicIds.DiagnosticoFinalizado,
            EventTypes.DiagnosticoFinalizado,
            data,
            version: 2,
            cancellationToken);
    }

    public Task PublishOrdemServicoAprovadaAsync(
        OrdemServicoAggregateRoot ordemServico,
        CancellationToken cancellationToken = default)
    {
        if (ordemServico.AprovadaEm is null)
        {
            throw new InvalidOperationException(
                $"Ordem de Serviço {ordemServico.Id} sem AprovadaEm ao publicar OrdemServicoAprovada.");
        }

        var itens = ordemServico.Servicos
            .Where(s => s.Status is StatusItemOrdemServico.Aprovado)
            .SelectMany(s => s.ItensNecessarios)
            .GroupBy(i => i.ItemEstoque.Id)
            .Select(g => new OrdemServicoAprovadaItem
            {
                ItemEstoqueId = g.Key,
                Quantidade = g.Sum(i => i.Quantidade)
            })
            .ToList();

        var data = new OrdemServicoAprovadaData
        {
            OrdemServicoId = ordemServico.Id,
            AprovadaEm = new DateTimeOffset(DateTime.SpecifyKind(ordemServico.AprovadaEm.Value, DateTimeKind.Utc)),
            Itens = itens
        };

        return eventPublisher.PublishAsync(
            TopicIds.OrdemServicoAprovada,
            EventTypes.OrdemServicoAprovada,
            data,
            cancellationToken);
    }

    public Task PublishOrdemServicoFinalizadaAsync(
        OrdemServicoAggregateRoot ordemServico,
        CancellationToken cancellationToken = default)
    {
        var data = new OrdemServicoFinalizadaData { OrdemServicoId = ordemServico.Id };
        return eventPublisher.PublishAsync(
            TopicIds.OrdemServicoFinalizada,
            EventTypes.OrdemServicoFinalizada,
            data,
            cancellationToken);
    }

    public Task PublishOrdemServicoDescartadaAsync(
        OrdemServicoAggregateRoot ordemServico,
        CancellationToken cancellationToken = default)
    {
        var data = new OrdemServicoDescartadaData { OrdemServicoId = ordemServico.Id };
        return eventPublisher.PublishAsync(
            TopicIds.OrdemServicoDescartada,
            EventTypes.OrdemServicoDescartada,
            data,
            cancellationToken);
    }

    private static ServicoDiagnosticoStatus MapServicoStatus(StatusItemOrdemServico status) =>
        status switch
        {
            StatusItemOrdemServico.Sugerido => ServicoDiagnosticoStatus.Sugerido,
            StatusItemOrdemServico.Aprovado => ServicoDiagnosticoStatus.Aprovado,
            StatusItemOrdemServico.Rejeitado => ServicoDiagnosticoStatus.Rejeitado,
            StatusItemOrdemServico.Concluido => ServicoDiagnosticoStatus.Concluido,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}

using Application.UseCases.Responses;
using Domain.Entities;

namespace Application.UseCases;

public class OrdemServicoResponse
{
    public Guid Id { get; init; }
    public StatusOrdemServico Status { get; init; }
    public decimal ValorTotal { get; init; }
    public DateTime RecebidaEm { get; init; }
    public DateTime? EntregueEm { get; init; }
    public DateTime? DescartadaEm { get; init; }
    public DateTime? AprovadaEm { get; init; }
    public required ClienteOrdemServicoResponse Cliente { get; init; }
    public required VeiculoOrdemServicoResponse Veiculo { get; init; }
    public required List<ServicoOrdemServicoResponse> Servicos { get; init; }
    public required List<ItemNecessarioResponse> ItensNecessariosParaExecucao { get; init; }

    public static OrdemServicoResponse From(OrdemServicoAggregateRoot ordemServico)
    {
        return new OrdemServicoResponse
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            ValorTotal = ordemServico.ValorTotal,
            RecebidaEm = ordemServico.RecebidaEm,
            EntregueEm = ordemServico.EntregueEm,
            DescartadaEm = ordemServico.DescartadaEm,
            AprovadaEm = ordemServico.AprovadaEm,
            Cliente = ClienteOrdemServicoResponse.From(ordemServico.Cliente),
            Veiculo = VeiculoOrdemServicoResponse.From(ordemServico.Veiculo),
            Servicos = ServicoOrdemServicoResponse.FromMany(ordemServico.Servicos),
            ItensNecessariosParaExecucao = ItemNecessarioResponse.FromMany(ordemServico.ItensNecessariosParaExecucao)
        };
    }
}

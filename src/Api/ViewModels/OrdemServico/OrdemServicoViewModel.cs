using Domain.Entities;

namespace Api.ViewModels.OrdemServico;

public record OrdemServicoViewModel
{
    public required Guid Id { get; init; }
    public required StatusOrdemServico Status { get; init; }
    public required decimal ValorTotal { get; init; }
    public required DateTime RecebidaEm { get; init; }
    public DateTime? EntregueEm { get; init; }
    public DateTime? DescartadaEm { get; init; }
    public DateTime? AprovadaEm { get; init; }
    public required ClienteOrdemServicoViewModel Cliente { get; init; }
    public required VeiculoOrdemServicoViewModel Veiculo { get; init; }
    public required List<ServicoOrdemServicoViewModel> Servicos { get; init; }
    public required List<ItemNecessarioViewModel> ItensNecessariosParaExecucao { get; init; }
}

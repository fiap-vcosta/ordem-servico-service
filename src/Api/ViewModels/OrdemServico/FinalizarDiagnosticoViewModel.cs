using Domain.Entities;

namespace Api.ViewModels.OrdemServico;

public record FinalizarDiagnosticoViewModel
{
    public required int Id { get; init; }
    public required StatusOrdemServico Status { get; init; }
    public required decimal ValorTotal { get; init; }
    public required DateTime RecebidaEm { get; init; }
    public required ClienteOrdemServicoViewModel Cliente { get; init; }
    public required VeiculoOrdemServicoViewModel Veiculo { get; init; }
    public required List<ServicoOrdemServicoViewModel> Servicos { get; init; }
}

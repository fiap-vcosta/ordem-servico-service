using Application.UseCases.Responses;
using Domain.Entities;

namespace Api.ViewModels.OrdemServico;

public record ServicoOrdemServicoViewModel
{
    public required int Id { get; init; }
    public required int IdOrdemServico { get; init; }
    public required StatusItemOrdemServico Status { get; init; }
    public DateTime? AprovadoEm { get; init; }
    public DateTime? RejeitadoEm { get; init; }
    public DateTime? ExecucaoIniciadaEm { get; init; }
    public DateTime? ExecucaoFinalizadaEm { get; init; }
    public required string Nome { get; init; }
    public required decimal ValorCobrado { get; init; }
    public required ServicoCatalogoViewModel ServicoCatalogo { get; init; }
    public required List<ItemNecessarioViewModel> ItensNecessarios { get; init; }

    public static ServicoOrdemServicoViewModel From(ServicoOrdemServicoResponse response)
    {
        return new ServicoOrdemServicoViewModel
        {
            Id = response.Id,
            IdOrdemServico = response.IdOrdemServico,
            Status = response.Status,
            AprovadoEm = response.AprovadoEm,
            RejeitadoEm = response.RejeitadoEm,
            ExecucaoIniciadaEm = response.ExecucaoIniciadaEm,
            ExecucaoFinalizadaEm = response.ExecucaoFinalizadaEm,
            Nome = response.Nome,
            ValorCobrado = response.ValorCobrado,
            ServicoCatalogo = ServicoCatalogoViewModel.From(response.ServicoCatalogo),
            ItensNecessarios = ItemNecessarioViewModel.FromMany(response.ItensNecessarios)
        };
    }

    public static List<ServicoOrdemServicoViewModel> FromMany(IEnumerable<ServicoOrdemServicoResponse> responses)
    {
        return responses.Select(From).ToList();
    }
}

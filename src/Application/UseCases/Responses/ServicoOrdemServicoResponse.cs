using Domain.Entities;

namespace Application.UseCases.Responses;

public class ServicoOrdemServicoResponse
{
    public int Id { get; init; }
    public int IdOrdemServico { get; init; }
    public StatusItemOrdemServico Status { get; init; }
    public DateTime? AprovadoEm { get; init; }
    public DateTime? RejeitadoEm { get; init; }
    public DateTime? ExecucaoIniciadaEm { get; init; }
    public DateTime? ExecucaoFinalizadaEm { get; init; }
    public string Nome { get; init; } = string.Empty;
    public decimal ValorCobrado { get; init; }
    public required ServicoCatalogoResponse ServicoCatalogo { get; init; }
    public required List<ItemNecessarioResponse> ItensNecessarios { get; init; }

    public static ServicoOrdemServicoResponse From(Servico servico)
    {
        return new ServicoOrdemServicoResponse
        {
            Id = servico.Id,
            IdOrdemServico = servico.IdOrdemServico,
            Status = servico.Status,
            AprovadoEm = servico.AprovadoEm,
            RejeitadoEm = servico.RejeitadoEm,
            ExecucaoIniciadaEm = servico.ExecucaoIniciadaEm,
            ExecucaoFinalizadaEm = servico.ExecucaoFinalizadaEm,
            Nome = servico.Nome,
            ValorCobrado = servico.ValorCobrado,
            ServicoCatalogo = ServicoCatalogoResponse.From(servico.ServicoCatalogo),
            ItensNecessarios = ItemNecessarioResponse.FromMany(servico.ItensNecessarios)
        };
    }

    public static List<ServicoOrdemServicoResponse> FromMany(IEnumerable<Servico> servicos)
    {
        return servicos.Select(From).ToList();
    }
}

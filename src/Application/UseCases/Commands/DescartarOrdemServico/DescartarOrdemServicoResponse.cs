using Application.UseCases.Responses;
using Domain.Entities;

namespace Application.UseCases.Commands.DescartarOrdemServico;

public class DescartarOrdemServicoResponse
{
    public int Id { get; init; }
    public StatusOrdemServico Status { get; init; }
    public DateTime RecebidaEm { get; init; }
    public DateTime DescartadaEm { get; init; }
    public required List<ServicoOrdemServicoResponse> Itens { get; init; }
    public required ClienteOrdemServicoResponse Cliente { get; init; }
    public required VeiculoOrdemServicoResponse Veiculo { get; init; }
}

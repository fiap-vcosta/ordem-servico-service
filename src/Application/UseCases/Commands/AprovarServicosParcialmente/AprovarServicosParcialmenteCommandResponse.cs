using Application.UseCases.Responses;
using Domain.Entities;

namespace Application.UseCases.Commands.AprovarServicosParcialmente;

public class AprovarServicosParcialmenteCommandResponse
{
    public Guid Id { get; init; }
    public StatusOrdemServico Status { get; init; }
    public decimal ValorTotal { get; init; }
    public DateTime RecebidaEm { get; init; }
    public required ClienteOrdemServicoResponse Cliente { get; init; }
    public required VeiculoOrdemServicoResponse Veiculo { get; init; }
    public required List<ServicoOrdemServicoResponse> Servicos { get; init; }
}

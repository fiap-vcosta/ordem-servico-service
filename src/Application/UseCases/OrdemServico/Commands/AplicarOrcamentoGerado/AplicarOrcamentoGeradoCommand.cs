using MediatR;

namespace Application.UseCases.OrdemServico.Commands.AplicarOrcamentoGerado;

public class AplicarOrcamentoGeradoCommand : IRequest<OrdemServicoResponse>
{
    public required int IdOrdemServico { get; init; }
    public required int VersaoDiagnostico { get; init; }
}

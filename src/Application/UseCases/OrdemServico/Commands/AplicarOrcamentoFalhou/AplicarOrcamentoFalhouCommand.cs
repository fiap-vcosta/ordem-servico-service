using MediatR;

namespace Application.UseCases.OrdemServico.Commands.AplicarOrcamentoFalhou;

public class AplicarOrcamentoFalhouCommand : IRequest<OrdemServicoResponse>
{
    public required int IdOrdemServico { get; init; }
    public required int VersaoDiagnostico { get; init; }
}

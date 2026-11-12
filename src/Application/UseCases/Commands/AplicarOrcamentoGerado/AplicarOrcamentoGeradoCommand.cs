using MediatR;

namespace Application.UseCases.Commands.AplicarOrcamentoGerado;

public class AplicarOrcamentoGeradoCommand : IRequest<OrdemServicoResponse>
{
    public required Guid IdOrdemServico { get; init; }
    public required int VersaoDiagnostico { get; init; }
}

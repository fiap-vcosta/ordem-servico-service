using MediatR;

namespace Application.UseCases.Commands.EnviarOrdemServicoParaDiagnostico;

public class EnviarOrdemServicoParaDiagnosticoCommand : IRequest
{
    public int IdOrdemServico { get; init; }
}

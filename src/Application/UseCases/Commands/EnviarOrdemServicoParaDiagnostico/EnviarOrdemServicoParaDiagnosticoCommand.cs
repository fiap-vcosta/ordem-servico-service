using MediatR;

namespace Application.UseCases.Commands.EnviarOrdemServicoParaDiagnostico;

public class EnviarOrdemServicoParaDiagnosticoCommand : IRequest
{
    public Guid IdOrdemServico { get; init; }
}

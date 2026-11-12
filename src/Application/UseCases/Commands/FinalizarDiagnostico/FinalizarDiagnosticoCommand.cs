using MediatR;

namespace Application.UseCases.Commands.FinalizarDiagnostico;

public class FinalizarDiagnosticoCommand : IRequest<FinalizarDiagnosticoCommandResponse>
{
    public Guid IdOrdemServico { get; init; }
}
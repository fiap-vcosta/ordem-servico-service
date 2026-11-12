using Domain.ValueObjects;
using MediatR;

namespace Application.UseCases.Commands.ConfirmarExecucaoOrdemServico;

public class ConfirmarExecucaoOrdemServicoCommand : IRequest<OrdemServicoResponse>
{
    public Guid IdOrdemServico { get; init; }
    public List<ServicoExecutado> ServicosExecutados { get; init; } = [];
}

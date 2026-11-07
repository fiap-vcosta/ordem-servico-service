using Domain.ValueObjects;

namespace Api.Controllers.Staff.OrdemServico.ConfirmarExecucao;

public class ConfirmarExecucaoRequest
{
    public List<ServicoExecutado> ServicosExecutados { get; init; } = [];
}
using Api.Contracts.Validation;

namespace Api.Controllers.Staff.OrdemServico.ConfirmarExecucao;

public class ConfirmarExecucaoRequestValidator : IValidator<ConfirmarExecucaoRequest>
{
    public ValidationResult Validate(ConfirmarExecucaoRequest request)
    {
        var result = new ValidationResult();

        if (request.ServicosExecutados is null or { Count: 0 })
        {
            result.Errors.Add("Pelo menos um serviço deve ter sido executado.");
            return result;
        }

        foreach (var servicoExecutado in request.ServicosExecutados)
        {
            if (servicoExecutado.IdServico <= 0)
            {
                result.Errors.Add($"Id {servicoExecutado.IdServico} inválido para serviço");
            }

            if (servicoExecutado.FinalizadoEm.Subtract(servicoExecutado.IniciadoEm).Minutes < 0)
            {
                result.Errors.Add($"Duração inválida para servico com id {servicoExecutado.IdServico} (deve ser maior que 1 minuto)");
            }
        }

        return result;
    }
}
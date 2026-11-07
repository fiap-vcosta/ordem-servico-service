using Api.Contracts.Validation;

namespace Api.Controllers.Staff.OrdemServico.CriarOrdemServico;

public class CriarOrdemServicoRequestValidator : IValidator<CriarOrdemServicoRequest>
{
    public ValidationResult Validate(CriarOrdemServicoRequest request)
    {
        var result = new ValidationResult();

        if (request.IdVeiculo <= 0)
        {
            result.Errors.Add("IdVeiculo deve ser um veículo válido.");
        }

        foreach (var servico in request.Servicos)
        {
            if (servico.IdServico <= 0)
            {
                result.Errors.Add("Servicos.IdServico deve ser um id válido.");
            }

            if (servico.ValorCobrado <= 0)
            {
                result.Errors.Add("Servicos.ValorCobrado deve ser maior que 0.");
            }

            foreach (var itemNecessario in servico.ItensNecessarios)
            {
                if (itemNecessario.IdItemEstoque <= 0)
                {
                    result.Errors.Add("Servicos.ItensNecessarios.IdItemEstoque deve ser um id válido.");
                }

                if (itemNecessario.Quantidade <= 0)
                {
                    result.Errors.Add("Servicos.ItensNecessarios.Quantidade deve ser maior que 0.");
                }
            }
        }

        return result;
    }
}

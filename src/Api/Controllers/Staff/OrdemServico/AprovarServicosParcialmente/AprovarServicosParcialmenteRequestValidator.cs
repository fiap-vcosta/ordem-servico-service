using Api.Contracts.Validation;

namespace Api.Controllers.Staff.OrdemServico.AprovarServicosParcialmente;

public class AprovarServicosParcialmenteRequestValidator : IValidator<AprovarServicosParcialmenteRequest>
{
    public ValidationResult Validate(AprovarServicosParcialmenteRequest request)
    {
        var result = new ValidationResult();

        if (request.IdsServicosAprovados is null or { Count: 0 })
        {
            result.Errors.Add("Pelo menos um serviço deve ser aprovado.");
        }

        return result;
    }
}
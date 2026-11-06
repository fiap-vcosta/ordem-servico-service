using Api.Contracts.Validation;

namespace Api.Controllers.Staff.ItemEstoque.RegistrarEntradaEstoque;

public class RegistrarEntradaEstoqueRequestValidator : IValidator<RegistrarEntradaEstoqueRequest>
{
    public ValidationResult Validate(RegistrarEntradaEstoqueRequest request)
    {
        var result = new ValidationResult();

        if (request.Quantidade <= 0)
        {
            result.Errors.Add("Quantidade deve ser maior que zero.");
        }

        return result;
    }
}

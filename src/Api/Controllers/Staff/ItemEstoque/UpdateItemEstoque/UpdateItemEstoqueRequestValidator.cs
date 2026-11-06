using Api.Contracts.Validation;
using Domain.Estoque.Entities;

namespace Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;

public class UpdateItemEstoqueRequestValidator : IValidator<UpdateItemEstoqueRequest>
{
    public ValidationResult Validate(UpdateItemEstoqueRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Codigo))
        {
            result.Errors.Add("Código não pode estar vazio.");
        }

        if (!Enum.IsDefined(typeof(ItemTipo), request.Tipo))
        {
            result.Errors.Add("Tipo de item inválido.");
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            result.Errors.Add("Nome não pode estar vazio.");
        }

        if (!Enum.IsDefined(typeof(UnidadeMedida), request.UnidadeMedida))
        {
            result.Errors.Add("Unidade de medida inválida.");
        }

        if (request.PrecoVenda <= 0)
        {
            result.Errors.Add("Preço de venda deve ser maior que zero.");
        }

        return result;
    }
}

using Domain.Estoque.Entities;

namespace Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;

public class CreateItemEstoqueRequest
{
    public string Codigo { get; init; } = string.Empty;
    public ItemTipo Tipo { get; init; }
    public string Nome { get; init; } = string.Empty;
    public UnidadeMedida UnidadeMedida { get; init; }
    public decimal PrecoVenda { get; init; }
    public decimal Saldo { get; init; }
    public decimal SaldoReservado { get; init; }
}

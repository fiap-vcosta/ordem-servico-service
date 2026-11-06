namespace Domain.ValueObjects;

public class ItemEstoqueOrdemServico
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string UnidadeMedida { get; init; } = string.Empty;
}
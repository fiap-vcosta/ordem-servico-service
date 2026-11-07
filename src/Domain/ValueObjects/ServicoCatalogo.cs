namespace Domain.ValueObjects;

public class ServicoCatalogo
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
}
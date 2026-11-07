namespace Domain.ValueObjects;

public class ClienteOrdemServico
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Documento { get; init; } = string.Empty;
}

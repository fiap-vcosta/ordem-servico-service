namespace Domain.ValueObjects;

public class ServicoExecutado
{
    public int IdServico {  get; init; }
    public DateTime IniciadoEm { get; init; }
    public DateTime FinalizadoEm { get; init; }
}
namespace Domain.Entities;

public class EventoProcessado
{
    public Guid EventId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string ChaveNegocio { get; set; } = string.Empty;

    public DateTimeOffset ProcessadoEm { get; set; }
}

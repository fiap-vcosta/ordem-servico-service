namespace Application.Time;

public static class AprovacaoTimestamp
{
    public static bool EqualsPersisted(DateTime aprovadaEmPersistida, DateTimeOffset aprovadaEmEvento)
    {
        var persistida = TruncateToMicroseconds(
            new DateTimeOffset(DateTime.SpecifyKind(aprovadaEmPersistida, DateTimeKind.Utc)));
        return persistida == TruncateToMicroseconds(aprovadaEmEvento);
    }

    public static DateTimeOffset ToEventTimestamp(DateTime aprovadaEm) =>
        TruncateToMicroseconds(new DateTimeOffset(DateTime.SpecifyKind(aprovadaEm, DateTimeKind.Utc)));

    public static DateTimeOffset TruncateToMicroseconds(DateTimeOffset value) =>
        new(value.Ticks - (value.Ticks % TimeSpan.TicksPerMicrosecond), value.Offset);
}

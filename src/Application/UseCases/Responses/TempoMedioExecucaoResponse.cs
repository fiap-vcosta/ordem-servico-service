namespace Application.UseCases.Responses;

public record TempoMedioExecucaoResponse
{
    public required int IdServico { get; init; }
    public required int TotalExecucoes { get; init; }
    public required TimeSpan ExecucaoMedia { get; init; }

    public static TempoMedioExecucaoResponse From(int idServico, int totalExecucoes, TimeSpan execucaoMedia)
    {
        return new TempoMedioExecucaoResponse
        {
            IdServico = idServico,
            TotalExecucoes = totalExecucoes,
            ExecucaoMedia = execucaoMedia
        };
    }
}

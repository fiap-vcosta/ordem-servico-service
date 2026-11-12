using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Logging;

public static partial class OrdemServicoStatusLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "OS {OrdemServicoId} movida para o status {Status}")]
    public static partial void LogStatusMovido(this ILogger logger, Guid ordemServicoId, StatusOrdemServico status);
}

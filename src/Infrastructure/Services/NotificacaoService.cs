using Application.Abstractions.Services;

namespace Infrastructure.Services;

public class NotificacaoService : INotificacaoService
{
    public Task NotificarUsuariosPorTipo(string tipoUsuario, string conteudo)
    {
        Console.WriteLine($"[Notificacao]<{tipoUsuario}>: {conteudo}");
        return Task.CompletedTask;
    }
}

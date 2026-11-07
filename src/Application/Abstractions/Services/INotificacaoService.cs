namespace Application.Abstractions.Services;

public interface INotificacaoService
{
    Task NotificarUsuariosPorTipo(string tipoUsuario, string conteudo);
}

using Infrastructure.Services;

namespace UnitTests.Infrastructure.Services;

public class NotificacaoServiceTests
{
    [Fact]
    public async Task NotificarUsuariosPorTipo_CompletesSuccessfully()
    {
        // Arrange
        var service = new NotificacaoService();

        // Act && Assert method completes without exception
        await service.NotificarUsuariosPorTipo("Mecanico", "Teste de notificação");
    }
}

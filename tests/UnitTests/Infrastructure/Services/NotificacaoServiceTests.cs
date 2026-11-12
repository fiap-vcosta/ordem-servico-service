using Infrastructure.Services;

namespace UnitTests.Infrastructure.Services;

public class NotificacaoServiceTests
{
    [Fact]
    public async Task NotificarUsuariosPorTipo_CompletesSuccessfully()
    {
        // Arrange
        var service = new NotificacaoService();
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act
            await service.NotificarUsuariosPorTipo("Mecanico", "Teste de notificação");

            // Assert
            var output = writer.ToString();
            Assert.Contains("[Notificacao]<Mecanico>", output, StringComparison.Ordinal);
            Assert.Contains("Teste de notificação", output, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}

using Infrastructure.Services;

namespace UnitTests.Infrastructure.Services;

public class SmtpServiceTests
{
    [Fact]
    public async Task EnviarEmail_CompletesSuccessfully()
    {
        // Arrange
        var service = new SmtpService();
        var email = "teste@teste.com";
        var conteudo = "Teste de corpo";
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act
            await service.EnviarEmail(email, conteudo);

            // Assert
            var output = writer.ToString();
            Assert.Contains($"[SMTP]<{email}>", output, StringComparison.Ordinal);
            Assert.Contains(conteudo, output, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}

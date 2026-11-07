using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure.Database.Gateways;

public class EventoProcessadoGatewayTests
{
    [Fact]
    public async Task ExisteEventIdAsync_ReturnsTrue_WhenPersisted()
    {
        // Arrange
        await using var context = CreateContext();
        var gateway = new EventoProcessadoGateway(context);
        var eventId = Guid.NewGuid();
        await gateway.AddAsync(new EventoProcessado
        {
            EventId = eventId,
            Tipo = "OrcamentoGerado",
            ChaveNegocio = "1:1",
            ProcessadoEm = DateTimeOffset.UtcNow
        });

        // Act
        var exists = await gateway.ExisteEventIdAsync(eventId);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExisteChaveNegocioAsync_ReturnsTrue_WhenPersisted()
    {
        // Arrange
        await using var context = CreateContext();
        var gateway = new EventoProcessadoGateway(context);
        await gateway.AddAsync(new EventoProcessado
        {
            EventId = Guid.NewGuid(),
            Tipo = "EstoqueReservado",
            ChaveNegocio = "10:key",
            ProcessadoEm = DateTimeOffset.UtcNow
        });

        // Act
        var exists = await gateway.ExisteChaveNegocioAsync("EstoqueReservado", "10:key");

        // Assert
        Assert.True(exists);
        Assert.False(await gateway.ExisteChaveNegocioAsync("EstoqueReservado", "other"));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
